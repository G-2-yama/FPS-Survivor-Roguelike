using System;
using System.Collections.Generic;

namespace BalanceSim
{
    public enum WeaponPhase
    {
        Idle,
        Firing,
        Cooldown,
        Reloading,
    }

    public sealed class WeaponState
    {
        private float _timer;
        private int _burstRemaining;
        private float _nextFireTime;
        private float _burstStartTime;
        private float _cooldownEndTime;
        private float? _resumeTime;
        private int _updateCount;
        private int _cooldownEnterUpdate;

        public SimWeaponDef Def { get; private set; }
        public int Ammo { get; private set; }
        public WeaponPhase Phase { get; private set; }
        public bool HasWeapon => Def != null;

        public void Equip(SimWeaponDef def, float time)
        {
            Def = def;
            Ammo = def != null ? def.magazineSize : 0;
            ChangeState(WeaponPhase.Idle, time);
        }

        public void LevelUp(SimWeaponDef def)
        {
            if (HasWeapon)
            {
                Def = def;
            }
        }

        public void OnFire(float time)
        {
            if (Phase == WeaponPhase.Idle)
            {
                ChangeState(WeaponPhase.Firing, time);
            }
        }

        public void OnReload(float time)
        {
            if (Phase == WeaponPhase.Idle)
            {
                ChangeState(WeaponPhase.Reloading, time);
            }
        }

        public void Update(float time, float dt, bool pressed, Action<SimWeaponDef> fire)
        {
            _updateCount++;
            WeaponPhase phase;
            do
            {
                phase = Phase;
                Step(time, dt, pressed, fire);
            }
            while (Phase != phase && (Phase == WeaponPhase.Firing || Phase == WeaponPhase.Cooldown));
        }

        private void Step(float time, float dt, bool pressed, Action<SimWeaponDef> fire)
        {
            switch (Phase)
            {
                case WeaponPhase.Firing:
                    UpdateFiring(time, fire);
                    break;
                case WeaponPhase.Cooldown:
                    UpdateCooldown(time, pressed);
                    break;
                case WeaponPhase.Reloading:
                    _timer -= dt;
                    if (_timer <= 0f)
                    {
                        Ammo = Def.magazineSize;
                        ChangeState(WeaponPhase.Idle, time);
                    }
                    break;
            }
        }

        private void UpdateFiring(float time, Action<SimWeaponDef> fire)
        {
            float interval = Def.burstInterval;
            while (_burstRemaining > 0 && time >= _nextFireTime)
            {
                if (!TryFire(fire))
                {
                    EnterCooldown(_nextFireTime, time);
                    return;
                }

                _burstRemaining--;
                _nextFireTime += interval;
            }

            if (_burstRemaining <= 0)
            {
                EnterCooldown(_nextFireTime - interval, time);
            }
        }

        private void EnterCooldown(float lastShotTime, float time)
        {
            _cooldownEndTime = lastShotTime + Def.fireInterval;
            ChangeState(WeaponPhase.Cooldown, time);
        }

        private void UpdateCooldown(float time, bool pressed)
        {
            if (time < _cooldownEndTime)
            {
                return;
            }

            bool keepFiring = Def.fullAuto && pressed;
            if (!keepFiring && Def.autoReload && Ammo <= 0)
            {
                ChangeState(WeaponPhase.Reloading, time);
                return;
            }

            if (keepFiring || Def.autoFire)
            {
                if (_updateCount == _cooldownEnterUpdate && _cooldownEndTime <= _burstStartTime)
                {
                    return;
                }
                _resumeTime = _cooldownEndTime;
            }

            ChangeState(keepFiring ? WeaponPhase.Firing : WeaponPhase.Idle, time);
        }

        private bool TryFire(Action<SimWeaponDef> fire)
        {
            if (!HasWeapon || Ammo <= 0)
            {
                return false;
            }

            Ammo--;
            fire(Def);
            return true;
        }

        private void ChangeState(WeaponPhase phase, float time)
        {
            Phase = phase;
            switch (phase)
            {
                case WeaponPhase.Idle:
                    if (HasWeapon && Def.autoFire)
                    {
                        ChangeState(WeaponPhase.Firing, time);
                    }
                    break;
                case WeaponPhase.Firing:
                    _burstRemaining = Def.burstCount;
                    _nextFireTime = _resumeTime ?? time;
                    _resumeTime = null;
                    _burstStartTime = _nextFireTime;
                    break;
                case WeaponPhase.Cooldown:
                    _cooldownEnterUpdate = _updateCount;
                    break;
                case WeaponPhase.Reloading:
                    _timer = MathF.Max(Def.reloadTime, 0.01f);
                    break;
            }
        }
    }

    public sealed partial class SimRun
    {
        private const int TeamEnemy = 1 << 2;
        private const int TeamEnemyAmmo = 1 << 3;
        private const int TeamBoss = 1 << 4;
        private const float GridCellSize = 5f;
        private const float KnockbackDecay = 0.9f;

        private readonly WeaponState[] _weapons = new WeaponState[Progression.WeaponSlotCount];
        private readonly string[] _equipped = new string[Progression.WeaponSlotCount];
        private readonly List<PlayerProjectile> _projectiles = new();
        private readonly SpatialGrid _grid = new(GridCellSize);
        private readonly List<EnemyAgent> _queryResults = new();
        private readonly List<RayHit> _rayHits = new();
        private readonly List<Body> _overlaps = new();
        private readonly List<Body> _chainHits = new();
        private readonly List<Body> _staleKeys = new();
        private readonly HashSet<Body> _overlapSet = new();
        private readonly Dictionary<string, Vec2> _stepPositions = new();
        private readonly Dictionary<string, (int damage, float knockback)> _lastInitialized = new();
        private Action<SimWeaponDef> _fire;
        private Vec2 _aim = Vec2.Forward;
        private float _time;

        private readonly struct RayHit
        {
            public readonly Body Body;
            public readonly float Distance;

            public RayHit(Body body, float distance)
            {
                Body = body;
                Distance = distance;
            }
        }

        private void InitWeapons()
        {
            _fire = FireWeapon;
            _aim = _player.Facing;
            for (int i = 0; i < _weapons.Length; i++)
            {
                _weapons[i] = new WeaponState();
            }
            SyncWeaponSlots(0f);
        }

        private void SyncWeaponSlots(float time)
        {
            for (int i = 0; i < _weapons.Length; i++)
            {
                string name = _progression.WeaponSlots[i];
                if (name == _equipped[i])
                {
                    continue;
                }

                SimWeaponDef def = WeaponDefOf(name);
                if (def != null && _weapons[i].HasWeapon && IsLaterLevel(_equipped[i], name))
                {
                    _weapons[i].LevelUp(def);
                }
                else
                {
                    _weapons[i].Equip(def, time);
                }
                _equipped[i] = name;
            }
        }

        private bool IsLaterLevel(string current, string target)
        {
            SimWeaponDef def = WeaponDefOf(current);
            for (int i = 0; def != null && i < _data.Weapons.Count; i++)
            {
                if (def.next == target)
                {
                    return true;
                }
                def = WeaponDefOf(def.next);
            }
            return false;
        }

        private SimWeaponDef WeaponDefOf(string name) => name != null && _data.Weapons.TryGetValue(name, out SimWeaponDef def) ? def : null;

        private SimProjectileDef ProjectileDefOf(string name) => name != null && _data.Projectiles.TryGetValue(name, out SimProjectileDef def) ? def : null;

        private void UpdateWeapons(float time, float dt)
        {
            SyncWeaponSlots(time);

            EnemyAgent nearest = FindNearestTarget();
            bool engage = false;
            if (nearest != null)
            {
                Vec2 toNearest = nearest.Position - _player.Position;
                if (toNearest.SqrLength > 1e-8f)
                {
                    _aim = toNearest.Normalized;
                }
                engage = toNearest.SqrLength <= _options.fireRange * _options.fireRange;
            }

            foreach (WeaponState weapon in _weapons)
            {
                if (!weapon.HasWeapon)
                {
                    continue;
                }

                bool pressed = false;
                if (weapon.Def.type != SimWeaponType.AutoWeapon)
                {
                    pressed = engage && weapon.Ammo > 0;
                    if (weapon.Phase == WeaponPhase.Idle)
                    {
                        if (pressed)
                        {
                            weapon.OnFire(time);
                        }
                        else if (weapon.Ammo <= 0 && weapon.Def.type == SimWeaponType.Main)
                        {
                            weapon.OnReload(time);
                        }
                    }
                }

                weapon.Update(time, dt, pressed, _fire);
            }
        }

        private EnemyAgent FindNearestTarget()
        {
            EnemyAgent nearest = null;
            float minSqrDistance = float.PositiveInfinity;
            foreach (EnemyAgent enemy in _enemies)
            {
                if (enemy.Presence != Presence.Active || enemy.DeathTime >= 0f)
                {
                    continue;
                }

                float sqrDistance = (enemy.Position - _player.Position).SqrLength;
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    nearest = enemy;
                }
            }
            return nearest;
        }

        private Vec2 FireDirection(SimWeaponDef def)
        {
            float error = Uniform(-_options.aimError, _options.aimError);
            float spread = Uniform(-def.spreadAngle * 0.5f, def.spreadAngle * 0.5f);
            return _aim.RotateDegrees(error + spread);
        }

        private float Uniform(float min, float max) => min + (max - min) * (float)_random.NextDouble();

        private void FireWeapon(SimWeaponDef def)
        {
            SimFireModeDef mode = def.fireMode;
            switch (mode.kind)
            {
                case SimFireModeKind.HitScan:
                    FireHitScan(def, FireDirection(def));
                    break;
                case SimFireModeKind.Chain:
                    FireChain(def, FireDirection(def));
                    break;
                case SimFireModeKind.Projectile:
                    SpawnFromMode(def, FireDirection(def));
                    break;
                case SimFireModeKind.Surrounded:
                {
                    Vec2 baseDirection = FireDirection(def);
                    for (int i = 0; i < mode.projectileCount; i++)
                    {
                        SpawnFromMode(def, baseDirection.RotateDegrees(i * 360f / mode.projectileCount));
                    }
                    break;
                }
                case SimFireModeKind.Area:
                    FireArea(def);
                    break;
                case SimFireModeKind.AutoTarget:
                    FireAutoTarget(def);
                    break;
                case SimFireModeKind.Step:
                    FireStep(def);
                    break;
            }
        }

        private void FireHitScan(SimWeaponDef def, Vec2 direction)
        {
            SimFireModeDef mode = def.fireMode;
            int damage = (int)(_progression.DamageMultiplier * def.damage);
            float knockback = _progression.KnockbackMultiplier * def.knockback;
            CastRay(direction, mode.maxRange, mode.hitRadius);
            foreach (RayHit hit in _rayHits)
            {
                int team = TeamOf(hit.Body);
                if ((team & mode.targetTeam) == 0)
                {
                    continue;
                }

                Damage(hit.Body, damage, knockback);
                if (team != TeamEnemyAmmo)
                {
                    break;
                }
            }
        }

        private void FireChain(SimWeaponDef def, Vec2 direction)
        {
            SimFireModeDef mode = def.fireMode;
            int damage = (int)(_progression.DamageMultiplier * def.damage);
            CastRay(direction, mode.maxRange, mode.hitRadius);
            foreach (RayHit hit in _rayHits)
            {
                int team = TeamOf(hit.Body);
                if ((team & mode.targetTeam) == 0)
                {
                    continue;
                }

                Damage(hit.Body, damage, 0f);
                if (team == TeamEnemyAmmo)
                {
                    continue;
                }

                _chainHits.Clear();
                _chainHits.Add(hit.Body);
                Body current = hit.Body;
                for (int i = 0; i < mode.chainCount; i++)
                {
                    EnemyAgent next = FindChainTarget(current, mode);
                    if (next == null)
                    {
                        break;
                    }

                    _chainHits.Add(next);
                    Damage(next, damage, 0f);
                    current = next;
                }
                break;
            }
        }

        private EnemyAgent FindChainTarget(Body origin, SimFireModeDef mode)
        {
            _grid.Query(origin.Position, mode.chainRange + _data.MaxTargetRadius, _queryResults);
            EnemyAgent nearest = null;
            float minDistance = float.PositiveInfinity;
            foreach (EnemyAgent enemy in _queryResults)
            {
                if (enemy == origin || _chainHits.Contains(enemy))
                {
                    continue;
                }

                int team = TeamOf(enemy);
                if (team == TeamEnemyAmmo || (team & mode.targetTeam) == 0)
                {
                    continue;
                }

                float distance = (enemy.Position - origin.Position).Length;
                if (distance > mode.chainRange + enemy.Def.radius)
                {
                    continue;
                }

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }
            return nearest;
        }

        private void CastRay(Vec2 direction, float range, float hitRadius)
        {
            _rayHits.Clear();
            Vec2 origin = _player.Position;
            float wall = WallDistance(origin, direction, range);
            _grid.Query(origin + direction * (range * 0.5f), range * 0.5f + hitRadius + _data.MaxTargetRadius, _queryResults);
            foreach (EnemyAgent enemy in _queryResults)
            {
                TryRayHit(enemy, enemy.Def.radius + hitRadius, origin, direction, MathF.Min(range, wall));
            }
            foreach (ShotAgent shot in _shots)
            {
                if (shot.Alive)
                {
                    TryRayHit(shot, shot.Def.radius + hitRadius, origin, direction, MathF.Min(range, wall));
                }
            }
            _rayHits.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        }

        private void TryRayHit(Body body, float radius, Vec2 origin, Vec2 direction, float range)
        {
            Vec2 offset = body.Position - origin;
            float radiusSqr = radius * radius;
            if (offset.SqrLength <= radiusSqr)
            {
                return;
            }

            float along = Vec2.Dot(offset, direction);
            float perpendicularSqr = offset.SqrLength - along * along;
            if (along < 0f || perpendicularSqr > radiusSqr)
            {
                return;
            }

            float distance = along - MathF.Sqrt(radiusSqr - perpendicularSqr);
            if (distance <= range)
            {
                _rayHits.Add(new RayHit(body, distance));
            }
        }

        private float WallDistance(Vec2 origin, Vec2 direction, float range)
        {
            float step = _terrain.CellSize * 0.5f;
            for (float d = step; d <= range; d += step)
            {
                if (_terrain.IsBlocked(origin + direction * d))
                {
                    return d;
                }
            }
            return float.PositiveInfinity;
        }

        private void SpawnFromMode(SimWeaponDef def, Vec2 direction)
        {
            SimFireModeDef mode = def.fireMode;
            SimProjectileDef projectile = ProjectileDefOf(mode.projectile);
            if (projectile == null)
            {
                return;
            }

            Vec2 position = _player.Position + direction.LocalToWorld(mode.offsetX, mode.offsetZ);
            Spawn(projectile, def.damage, def.knockback, position, direction, mode.tracking, _input.player.eyeHeight, ScaleFor(projectile, def.damage));
        }

        private void FireArea(SimWeaponDef def)
        {
            SimFireModeDef mode = def.fireMode;
            SimProjectileDef projectile = ProjectileDefOf(mode.projectile);
            for (int i = 0; i < mode.projectileCount; i++)
            {
                float x = Uniform(-mode.areaSizeX * 0.5f, mode.areaSizeX * 0.5f);
                float z = Uniform(-mode.areaSizeZ * 0.5f, mode.areaSizeZ * 0.5f);
                if (projectile == null)
                {
                    continue;
                }

                Vec2 position = _player.Position + new Vec2(x, z);
                float height = _input.player.pivotHeight + mode.spawnHeight;
                Spawn(projectile, def.damage, def.knockback, position, Vec2.Forward, false, height, ScaleFor(projectile, def.damage));
            }
        }

        private void FireAutoTarget(SimWeaponDef def)
        {
            SimFireModeDef mode = def.fireMode;
            EnemyAgent target = null;
            float minSqrDistance = float.PositiveInfinity;
            foreach (EnemyAgent enemy in _enemies)
            {
                if (enemy.Presence != Presence.Active || (TeamOf(enemy) & (TeamEnemy | TeamBoss)) == 0)
                {
                    continue;
                }

                float sqrDistance = (enemy.Position - _player.Position).SqrLength;
                float reach = mode.searchRadius + enemy.Def.radius;
                if (sqrDistance <= reach * reach && sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    target = enemy;
                }
            }

            Vec2 direction = target != null ? (target.Position - _player.Position).Normalized : Vec2.Zero;
            if (direction.SqrLength == 0f)
            {
                direction = FireDirection(def);
            }
            SpawnFromMode(def, direction);
        }

        private void FireStep(SimWeaponDef def)
        {
            SimFireModeDef mode = def.fireMode;
            Vec2 previous = _stepPositions.TryGetValue(mode.name ?? string.Empty, out Vec2 position) ? position : Vec2.Zero;
            if ((_player.Position - previous).Length < mode.stepInterval)
            {
                return;
            }

            SimProjectileDef projectile = ProjectileDefOf(mode.projectile);
            if (projectile != null)
            {
                Vec2 spawnPosition = _player.Position + new Vec2(mode.offsetX, mode.offsetZ);
                Spawn(projectile, def.damage, def.knockback, spawnPosition, Vec2.Forward, false, _input.player.eyeHeight, ScaleFor(projectile, def.damage));
            }
            _stepPositions[mode.name ?? string.Empty] = _player.Position;
        }

        private static float ScaleFor(SimProjectileDef def, int damage)
        {
            if (def.scaleSteps.Count == 0)
            {
                return 1f;
            }

            float scale = def.defaultScale;
            int highest = int.MinValue;
            foreach (SimScaleStep step in def.scaleSteps)
            {
                if (damage >= step.damageThreshold && step.damageThreshold > highest)
                {
                    scale = step.scale;
                    highest = step.damageThreshold;
                }
            }
            return scale;
        }

        private void Spawn(SimProjectileDef def, int damage, float knockback, Vec2 position, Vec2 direction, bool tracking, float height, float scale)
        {
            var projectile = new PlayerProjectile
            {
                Def = def,
                Damage = damage,
                Knockback = knockback,
                Scale = scale,
                Position = position,
                Facing = direction,
                Direction = direction,
                Tracking = tracking,
                Height = height,
                ExpireTime = def.lifetime > 0f ? _time + def.lifetime : float.PositiveInfinity,
            };

            if (def.move.kind == SimProjectileMoveKind.Parabola)
            {
                projectile.Velocity = direction * def.move.speed;
                projectile.VerticalVelocity = def.move.upwardSpeed;
            }

            if (tracking)
            {
                Vec2 offset = position - _player.Position;
                projectile.LocalPosition = new Vec2(Vec2.Dot(offset, _aim.Right), Vec2.Dot(offset, _aim));
                projectile.LocalFacing = new Vec2(Vec2.Dot(direction, _aim.Right), Vec2.Dot(direction, _aim));
            }

            _projectiles.Add(projectile);
        }

        private void UpdateProjectiles(float time, float dt)
        {
            float maxDistanceSqr = _terrain.Period * _terrain.Period;
            int count = _projectiles.Count;
            for (int i = 0; i < count; i++)
            {
                PlayerProjectile projectile = _projectiles[i];
                if (!projectile.Alive)
                {
                    continue;
                }

                if (time >= projectile.ExpireTime)
                {
                    projectile.Alive = false;
                    continue;
                }

                Vec2 from = projectile.Position;
                MoveProjectile(projectile, dt);
                switch (projectile.Def.kind)
                {
                    case SimDamageKind.Object:
                        UpdateDamageObject(projectile, from);
                        break;
                    case SimDamageKind.Generator:
                        UpdateDamageGenerator(projectile, from);
                        break;
                    case SimDamageKind.Area:
                        UpdateDamageArea(projectile, dt);
                        break;
                }

                if (!projectile.Tracking && (projectile.Position - _player.Position).SqrLength > maxDistanceSqr)
                {
                    projectile.Alive = false;
                }
            }
        }

        private void MoveProjectile(PlayerProjectile projectile, float dt)
        {
            SimProjectileMoveDef move = projectile.Def.move;
            projectile.Age += dt;
            switch (move.kind)
            {
                case SimProjectileMoveKind.Straight:
                    projectile.Position += projectile.Direction * (move.speed * dt);
                    break;
                case SimProjectileMoveKind.Tracking:
                    UpdateHoming(projectile, move, dt);
                    break;
                case SimProjectileMoveKind.Boomerang:
                {
                    float t = projectile.Age / move.duration;
                    float x = MathF.Sin(t * MathF.PI * 2f) * move.width;
                    float z = MathF.Sin(t * MathF.PI) * move.length;
                    projectile.Position = _player.Position + projectile.Direction.LocalToWorld(x, z);
                    break;
                }
                case SimProjectileMoveKind.Parabola:
                    projectile.VerticalVelocity -= move.gravity * dt;
                    projectile.Height += projectile.VerticalVelocity * dt;
                    projectile.Position += projectile.Velocity * dt;
                    break;
                case SimProjectileMoveKind.Orbit:
                {
                    projectile.OrbitAngle += move.speed * dt;
                    float radians = projectile.OrbitAngle * (MathF.PI / 180f);
                    projectile.Position = _player.Position + new Vec2(MathF.Cos(radians), MathF.Sin(radians)) * move.radius;
                    break;
                }
                case SimProjectileMoveKind.Rotate:
                    if (projectile.Tracking)
                    {
                        projectile.LocalFacing = projectile.LocalFacing.RotateDegrees(move.speed * dt);
                    }
                    else
                    {
                        projectile.Facing = projectile.Facing.RotateDegrees(move.speed * dt);
                    }
                    break;
                case SimProjectileMoveKind.Fall:
                    projectile.VerticalVelocity -= Movement.Gravity * dt;
                    projectile.Height += projectile.VerticalVelocity * dt;
                    break;
            }

            if (projectile.Tracking && (move.kind == SimProjectileMoveKind.None || move.kind == SimProjectileMoveKind.Rotate))
            {
                projectile.Position = _player.Position + _aim.LocalToWorld(projectile.LocalPosition.X, projectile.LocalPosition.Z);
                projectile.Facing = _aim.LocalToWorld(projectile.LocalFacing.X, projectile.LocalFacing.Z);
            }
        }

        private void UpdateHoming(PlayerProjectile projectile, SimProjectileMoveDef move, float dt)
        {
            float rangeSqr = move.trackingRange * move.trackingRange;
            if (projectile.Target == null || !IsPresent(projectile.Target)
                || (projectile.Target.Position - projectile.Position).SqrLength > rangeSqr)
            {
                projectile.Target = FindNearestBody(projectile.Position, move.trackingRange, projectile.Def.targetTeam);
            }

            if (projectile.Target != null)
            {
                Vec2 toTarget = (projectile.Target.Position - projectile.Position).Normalized;
                projectile.Direction = RotateTowards(projectile.Direction, toTarget, move.rotateSpeed * dt);
            }

            projectile.Position += projectile.Direction * (move.speed * dt);
            projectile.Facing = projectile.Direction;
        }

        private static Vec2 RotateTowards(Vec2 from, Vec2 to, float maxDegrees)
        {
            if (to.SqrLength == 0f)
            {
                return from;
            }

            float cross = from.Z * to.X - from.X * to.Z;
            float angle = MathF.Atan2(cross, Vec2.Dot(from, to)) * (180f / MathF.PI);
            return from.RotateDegrees(Math.Clamp(angle, -maxDegrees, maxDegrees));
        }

        private Body FindNearestBody(Vec2 center, float range, int targetTeam)
        {
            Body nearest = null;
            float minSqrDistance = float.PositiveInfinity;
            _grid.Query(center, range + _data.MaxTargetRadius, _queryResults);
            foreach (EnemyAgent enemy in _queryResults)
            {
                Consider(enemy, enemy.Def.radius);
            }
            foreach (ShotAgent shot in _shots)
            {
                if (shot.Alive)
                {
                    Consider(shot, shot.Def.radius);
                }
            }
            return nearest;

            void Consider(Body body, float radius)
            {
                if ((TeamOf(body) & targetTeam) == 0)
                {
                    return;
                }

                float sqrDistance = (body.Position - center).SqrLength;
                float reach = range + radius;
                if (sqrDistance <= reach * reach && sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    nearest = body;
                }
            }
        }

        private void UpdateDamageObject(PlayerProjectile projectile, Vec2 from)
        {
            Body hit = FirstSweptTarget(from, projectile.Position, ProjectileRadius(projectile), projectile.Def.targetTeam);
            if (hit != null)
            {
                Damage(hit, projectile.Damage, projectile.Knockback);
                projectile.Alive = false;
                return;
            }

            if (projectile.Def.collides && (HitsWall(from, projectile.Position) || HitsGround(projectile)))
            {
                projectile.Alive = false;
            }
        }

        private void UpdateDamageGenerator(PlayerProjectile projectile, Vec2 from)
        {
            if (projectile.Def.move.kind != SimProjectileMoveKind.Fall)
            {
                Body hit = FirstSweptTarget(from, projectile.Position, ProjectileRadius(projectile), projectile.Def.targetTeam);
                if (hit != null)
                {
                    Damage(hit, projectile.Damage, projectile.Knockback);
                    Explode(projectile);
                    return;
                }
            }

            if (projectile.Def.collides && (HitsWall(from, projectile.Position) || HitsGround(projectile)))
            {
                Explode(projectile);
            }
        }

        private static bool HitsGround(PlayerProjectile projectile)
        {
            SimProjectileMoveKind kind = projectile.Def.move.kind;
            return (kind == SimProjectileMoveKind.Parabola || kind == SimProjectileMoveKind.Fall) && projectile.Height <= 0f;
        }

        private void Explode(PlayerProjectile projectile)
        {
            foreach (string name in projectile.Def.explosions)
            {
                SimProjectileDef explosion = ProjectileDefOf(name);
                if (explosion != null)
                {
                    Spawn(explosion, projectile.Damage, projectile.Knockback, projectile.Position, Vec2.Forward, false, 0f, ScaleFor(explosion, projectile.Damage));
                }
            }
            projectile.Alive = false;
        }

        private static float ProjectileRadius(PlayerProjectile projectile)
        {
            SimShapeDef shape = projectile.Def.shape;
            return (shape.circle ? shape.radius : (shape.halfX + shape.halfZ) * 0.5f) * projectile.Scale;
        }

        private Body FirstSweptTarget(Vec2 from, Vec2 to, float radius, int targetTeam)
        {
            Body first = null;
            float firstT = float.PositiveInfinity;
            Vec2 segment = to - from;
            float lengthSqr = segment.SqrLength;
            _grid.Query((from + to) * 0.5f, MathF.Sqrt(lengthSqr) * 0.5f + radius + _data.MaxTargetRadius, _queryResults);
            foreach (EnemyAgent enemy in _queryResults)
            {
                Consider(enemy, enemy.Def.radius);
            }
            foreach (ShotAgent shot in _shots)
            {
                if (shot.Alive)
                {
                    Consider(shot, shot.Def.radius);
                }
            }
            return first;

            void Consider(Body body, float bodyRadius)
            {
                if ((TeamOf(body) & targetTeam) == 0)
                {
                    return;
                }

                float t = lengthSqr > 0f ? Math.Clamp(Vec2.Dot(body.Position - from, segment) / lengthSqr, 0f, 1f) : 0f;
                Vec2 closest = from + segment * t;
                float reach = radius + bodyRadius;
                if ((body.Position - closest).SqrLength < reach * reach && t < firstT)
                {
                    firstT = t;
                    first = body;
                }
            }
        }

        private void UpdateDamageArea(PlayerProjectile projectile, float dt)
        {
            CollectOverlaps(projectile);
            if (projectile.Def.areaMode == SimAreaMode.OnEnterOnce)
            {
                foreach (Body body in _overlaps)
                {
                    if (!projectile.Inside.Contains(body))
                    {
                        Damage(body, projectile.Damage, projectile.Knockback);
                    }
                }
                projectile.Inside.Clear();
                projectile.Inside.UnionWith(_overlaps);
                return;
            }

            foreach (Body body in _overlaps)
            {
                float timer = projectile.Timers.TryGetValue(body, out float value) ? value : 0f;
                timer += dt;
                if (timer >= projectile.Def.interval)
                {
                    Damage(body, projectile.Damage, projectile.Knockback);
                    timer = 0f;
                }
                projectile.Timers[body] = timer;
            }

            if (projectile.Timers.Count > _overlaps.Count)
            {
                _overlapSet.Clear();
                _overlapSet.UnionWith(_overlaps);
                _staleKeys.Clear();
                foreach (Body body in projectile.Timers.Keys)
                {
                    if (!_overlapSet.Contains(body))
                    {
                        _staleKeys.Add(body);
                    }
                }
                foreach (Body body in _staleKeys)
                {
                    projectile.Timers.Remove(body);
                }
            }
        }

        private void CollectOverlaps(PlayerProjectile projectile)
        {
            _overlaps.Clear();
            SimShapeDef shape = projectile.Def.shape;
            float scale = projectile.Scale;
            Vec2 center = projectile.Position + projectile.Facing.LocalToWorld(shape.centerX * scale, shape.centerZ * scale);
            float reach = (shape.circle ? shape.radius : MathF.Sqrt(shape.halfX * shape.halfX + shape.halfZ * shape.halfZ)) * scale;
            _grid.Query(center, reach + _data.MaxTargetRadius, _queryResults);
            foreach (EnemyAgent enemy in _queryResults)
            {
                if ((TeamOf(enemy) & projectile.Def.targetTeam) != 0 && Overlaps(shape, scale, center, projectile.Facing, enemy.Position, enemy.Def.radius))
                {
                    _overlaps.Add(enemy);
                }
            }
            foreach (ShotAgent shot in _shots)
            {
                if (shot.Alive && (TeamOf(shot) & projectile.Def.targetTeam) != 0 && Overlaps(shape, scale, center, projectile.Facing, shot.Position, shot.Def.radius))
                {
                    _overlaps.Add(shot);
                }
            }
        }

        private static bool Overlaps(SimShapeDef shape, float scale, Vec2 center, Vec2 facing, Vec2 point, float radius)
        {
            Vec2 offset = point - center;
            if (shape.circle)
            {
                float reach = shape.radius * scale + radius;
                return offset.SqrLength < reach * reach;
            }

            float halfX = shape.halfX * scale;
            float halfZ = shape.halfZ * scale;
            float localX = Vec2.Dot(offset, facing.Right);
            float localZ = Vec2.Dot(offset, facing);
            float dx = localX - Math.Clamp(localX, -halfX, halfX);
            float dz = localZ - Math.Clamp(localZ, -halfZ, halfZ);
            return dx * dx + dz * dz < radius * radius;
        }

        private static int TeamOf(Body body)
        {
            return body is EnemyAgent enemy && enemy.Def.hasEnemyComponent ? TeamEnemy : TeamEnemyAmmo;
        }

        private static bool IsPresent(Body body)
        {
            return body switch
            {
                EnemyAgent enemy => enemy.Presence == Presence.Active,
                ShotAgent shot => shot.Alive,
                _ => false,
            };
        }

        private void Damage(Body body, int damage, float knockback)
        {
            switch (body)
            {
                case EnemyAgent enemy:
                    DamageEnemy(enemy, damage, knockback, _time);
                    break;
                case ShotAgent shot:
                    DamageShot(shot, damage);
                    break;
            }
        }

        private static void DamageShot(ShotAgent shot, int damage)
        {
            int applied = shot.Def.damageRange * damage;
            if (!shot.Alive || applied <= 0)
            {
                return;
            }

            shot.Hp = Math.Max(shot.Hp - applied, 0);
            if (shot.Hp == 0)
            {
                shot.Alive = false;
            }
        }

        private void SpawnDeathExplosion(EnemyAgent enemy)
        {
            SimProjectileDef def = ProjectileDefOf(enemy.Def.deathExplosion);
            if (def == null)
            {
                return;
            }

            int damage = (int)(enemy.FinalDamage * enemy.Def.destroyDamageDamping);
            float knockback = 0f;
            float scale;
            if (damage > 0)
            {
                _lastInitialized[def.name] = (damage, 0f);
                scale = ScaleFor(def, damage);
            }
            else
            {
                (damage, knockback) = _lastInitialized.TryGetValue(def.name, out var last) ? last : (def.damage, def.knockback);
                scale = def.scaleSteps.Count > 0 ? def.defaultScale : 1f;
            }

            Spawn(def, damage, knockback, enemy.Position, Vec2.Forward, false, 0f, scale);
        }
    }
}
