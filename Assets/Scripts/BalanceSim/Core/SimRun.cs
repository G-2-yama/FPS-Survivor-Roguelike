using System;
using System.Collections.Generic;

namespace BalanceSim
{
    public sealed class GeneratorState
    {
        public SimGeneratorDef Def;
        public List<SimPhase> Phases;
        public int PhaseIndex;
        public float NextSpawnTime;
        public readonly List<EnemyAgent> ActiveEnemies = new();

        public SimPhase CurrentPhase => Phases[PhaseIndex];
    }

    public sealed class SimData
    {
        public SimInput Input { get; }
        public Terrain Terrain { get; }
        public Dictionary<string, SimEnemyDef> Enemies { get; } = new();
        public Dictionary<string, SimPickupDef> Pickups { get; } = new();
        public Dictionary<string, SimWeaponDef> Weapons { get; } = new();
        public Dictionary<string, SimProjectileDef> Projectiles { get; } = new();
        public float MaxTargetRadius { get; }

        public SimData(SimInput input)
        {
            Input = input;
            Terrain = new Terrain(input.terrain);
            foreach (SimEnemyDef enemy in input.enemies)
            {
                Enemies[enemy.name] = enemy;
                MaxTargetRadius = MathF.Max(MaxTargetRadius, enemy.radius);
                MaxTargetRadius = MathF.Max(MaxTargetRadius, enemy.attack.shot.radius);
            }
            foreach (SimPickupDef pickup in input.pickups)
            {
                Pickups[pickup.name] = pickup;
            }
            foreach (SimWeaponDef weapon in input.weapons)
            {
                Weapons[weapon.name] = weapon;
            }
            foreach (SimProjectileDef projectile in input.projectiles)
            {
                Projectiles[projectile.name] = projectile;
            }
        }
    }

    public sealed class SimRunOutput
    {
        public SimRunSummary Summary;
        public float[][] Samples;
    }

    public sealed partial class SimRun
    {
        public const int FixedSeriesCount = 4;
        private const int EscapeDirections = 16;
        private const float SpawnCheckRadius = 0.5f;

        private readonly SimData _data;
        private readonly SimInput _input;
        private readonly SimOptions _options;
        private readonly Terrain _terrain;
        private readonly Random _random;
        private readonly int _seed;
        private readonly PlayerState _player;
        private readonly Progression _progression;
        private readonly List<GeneratorState> _generators = new();
        private readonly List<EnemyAgent> _enemies = new();
        private readonly List<ShotAgent> _shots = new();
        private readonly List<AreaAgent> _areas = new();
        private readonly List<PickupAgent> _pickups = new();
        private readonly List<SpeedBuff> _buffs = new();
        private readonly float[] _dangerBins = new float[EscapeDirections];
        private readonly Vec2[] _escapeDirections = new Vec2[EscapeDirections];
        private readonly float[] _binWeights = new float[EscapeDirections];
        private readonly List<string> _evolvedWeapons;
        private readonly float[][] _samples;
        private int _nextSample;
        private int _gridX;
        private int _gridZ;
        private int _kills;

        public static int SampleCount(SimInput input) => (int)MathF.Floor(input.timeLimit / input.options.sampleInterval) + 1;

        public static List<SimSeries> SeriesDefs(SimInput input)
        {
            var defs = new List<SimSeries>
            {
                new() { name = "レベル", group = "レベル" },
                new() { name = "撃破数", group = "撃破数" },
                new() { name = "HP", group = "HP" },
                new() { name = "生存率", group = "生存率", ratio = true },
            };
            foreach (SimGeneratorDef generator in input.generators)
            {
                string group = $"場に残っている数 ({generator.name})";
                defs.Add(new SimSeries { name = $"{generator.name} の生存数", group = group });
                defs.Add(new SimSeries { name = $"{generator.name} のリスト数", group = group });
            }
            foreach (string weapon in Progression.EvolvedWeapons(input))
            {
                defs.Add(new SimSeries { name = $"{weapon} の進化率", group = "進化率", ratio = true });
            }
            return defs;
        }

        public SimRun(SimData data, IUpgradeChoicePolicy policy, int seed)
        {
            _data = data;
            _input = data.Input;
            _options = _input.options;
            _terrain = data.Terrain;
            _seed = seed;
            _random = new Random(seed);

            _player = new PlayerState(_input.player.maxHp)
            {
                Position = new Vec2(_input.player.x, _input.player.z),
                Radius = _input.player.radius,
                RunSpeed = _input.player.runSpeed,
            };
            _progression = new Progression(_input, _player, policy, _random);
            _gridX = _terrain.WorldToGrid(_player.Position.X);
            _gridZ = _terrain.WorldToGrid(_player.Position.Z);

            foreach (SimGeneratorDef def in _input.generators)
            {
                var phases = new List<SimPhase>(def.phases);
                phases.Sort((a, b) => a.startTime.CompareTo(b.startTime));
                _generators.Add(new GeneratorState { Def = def, Phases = phases });
            }

            for (int i = 0; i < EscapeDirections; i++)
            {
                _escapeDirections[i] = Vec2.Forward.RotateDegrees(360f / EscapeDirections * i);
            }

            for (int i = 0; i < EscapeDirections; i++)
            {
                float cos = Vec2.Dot(_escapeDirections[0], _escapeDirections[i]);
                _binWeights[i] = (1f + cos) * 0.5f;
            }

            _evolvedWeapons = Progression.EvolvedWeapons(_input);
            int seriesCount = FixedSeriesCount + _generators.Count * 2 + _evolvedWeapons.Count;
            int sampleCount = SampleCount(_input);
            _samples = new float[seriesCount][];
            for (int i = 0; i < seriesCount; i++)
            {
                _samples[i] = new float[sampleCount];
            }

            InitWeapons();
        }

        public SimRunOutput Execute()
        {
            float dt = _options.timeStep;
            bool cleared = false;
            float endTime = 0f;
            for (int step = 0; ; step++)
            {
                float time = step * dt;
                Record(time);
                if (time >= _input.timeLimit)
                {
                    cleared = true;
                    endTime = _input.timeLimit;
                    break;
                }

                Step(time, dt);
                if (_player.DeathTriggered)
                {
                    endTime = time;
                    break;
                }
            }

            FillRemainingSamples(cleared);
            return new SimRunOutput
            {
                Summary = new SimRunSummary
                {
                    seed = _seed,
                    cleared = cleared,
                    endTime = endTime,
                    level = _progression.Level,
                    kills = _kills,
                    levelUps = _progression.LevelUps,
                    evolutions = _progression.Evolutions,
                },
                Samples = _samples,
            };
        }

        private void Step(float time, float dt)
        {
            _time = time;
            foreach (GeneratorState generator in _generators)
            {
                UpdateGenerator(generator, time);
            }

            UpdateBuffs(dt);
            UpdatePlayer(dt);

            for (int i = 0; i < _enemies.Count; i++)
            {
                UpdateEnemy(_enemies[i], time, dt);
            }

            UpdateShots(dt);
            UpdateAreas(time);
            _grid.Rebuild(_enemies);
            UpdateWeapons(time, dt);
            UpdateProjectiles(time, dt);
            UpdatePickups(time, dt);
            UpdateContacts(time);
            ProcessDeaths(time);
            UpdateWindow();
            Compact();
        }

        private void UpdateGenerator(GeneratorState generator, float time)
        {
            if (generator.Phases.Count == 0)
            {
                return;
            }

            if (generator.PhaseIndex + 1 < generator.Phases.Count
                && time >= generator.Phases[generator.PhaseIndex + 1].startTime)
            {
                generator.PhaseIndex++;
            }

            if (time < generator.NextSpawnTime)
            {
                return;
            }

            SimPhase phase = generator.CurrentPhase;
            if (generator.ActiveEnemies.Count < phase.minSpawnCount)
            {
                int spawnCount = phase.minSpawnCount - generator.ActiveEnemies.Count;
                for (int i = 0; i < spawnCount; i++)
                {
                    SpawnEnemy(generator, phase);
                }
            }
            else
            {
                SpawnEnemy(generator, phase);
            }

            generator.NextSpawnTime = time + generator.CurrentPhase.spawnInterval;
        }

        private void SpawnEnemy(GeneratorState generator, SimPhase phase)
        {
            SimSpawnEntry entry = PickSpawnEntry(phase);
            if (entry == null || entry.prefabName == null || !_data.Enemies.TryGetValue(entry.prefabName, out SimEnemyDef def))
            {
                return;
            }

            for (int i = 0; i < generator.Def.maxSpawnTry; i++)
            {
                Vec2 direction = PickSpawnDirection(generator.Def);
                Vec2 position = _player.Position + direction * generator.Def.spawnRadius;

                if (generator.ActiveEnemies.Count >= generator.Def.maxEnemyCount)
                {
                    EnemyAgent farthest = FindFarthest(generator);
                    if (farthest != null)
                    {
                        generator.ActiveEnemies.Remove(farthest);
                        if (farthest.Def.hasEnemyComponent && farthest.Presence == Presence.Active)
                        {
                            Release(farthest);
                        }
                    }
                }

                if (IsSpawnable(position))
                {
                    var enemy = new EnemyAgent
                    {
                        Def = def,
                        Generator = generator,
                        Position = position,
                        Hp = def.maxHp,
                        IsThreat = def.touchDamage > 0 || def.attack.shotCount > 0 || def.contactArea.enabled,
                    };
                    generator.ActiveEnemies.Add(enemy);
                    _enemies.Add(enemy);
                    return;
                }
            }
        }

        private SimSpawnEntry PickSpawnEntry(SimPhase phase)
        {
            int total = 0;
            foreach (SimSpawnEntry entry in phase.enemies)
            {
                total += entry.spawnWeight;
            }

            int random = total > 0 ? _random.Next(total) : 0;
            int current = 0;
            foreach (SimSpawnEntry entry in phase.enemies)
            {
                current += entry.spawnWeight;
                if (random < current)
                {
                    return entry;
                }
            }
            return null;
        }

        private Vec2 PickSpawnDirection(SimGeneratorDef def)
        {
            float total = 0f;
            foreach (SimSpawnDirection direction in def.directions)
            {
                total += direction.weight;
            }

            float random = (float)_random.NextDouble() * total;
            float current = 0f;
            foreach (SimSpawnDirection direction in def.directions)
            {
                current += direction.weight;
                if (random < current)
                {
                    float angle = ((float)_random.NextDouble() * 2f - 1f) * direction.angle;
                    return _player.Facing.RotateDegrees(angle);
                }
            }
            return _player.Facing;
        }

        private EnemyAgent FindFarthest(GeneratorState generator)
        {
            EnemyAgent farthest = null;
            float maxSqrDistance = -1f;
            foreach (EnemyAgent enemy in generator.ActiveEnemies)
            {
                if (enemy.Presence == Presence.Released)
                {
                    continue;
                }

                float sqrDistance = enemy.Presence == Presence.Fallen
                    ? float.PositiveInfinity
                    : (enemy.Position - _player.Position).SqrLength;
                if (sqrDistance > maxSqrDistance)
                {
                    maxSqrDistance = sqrDistance;
                    farthest = enemy;
                }
            }
            return farthest;
        }

        private bool IsSpawnable(Vec2 position)
        {
            if (_terrain.IsBlocked(position))
            {
                return false;
            }

            foreach (EnemyAgent enemy in _enemies)
            {
                if (enemy.Presence != Presence.Active)
                {
                    continue;
                }

                float reach = SpawnCheckRadius + enemy.Def.radius;
                if ((enemy.Position - position).SqrLength < reach * reach)
                {
                    return false;
                }
            }

            foreach (PickupAgent pickup in _pickups)
            {
                float reach = SpawnCheckRadius + pickup.Def.radius;
                if (pickup.Alive && (pickup.Position - position).SqrLength < reach * reach)
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateBuffs(float dt)
        {
            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                _buffs[i].Remaining -= dt;
                if (_buffs[i].Remaining <= 0f)
                {
                    _player.RunSpeed -= _buffs[i].Amount;
                    _buffs.RemoveAt(i);
                }
            }
        }

        private void UpdatePlayer(float dt)
        {
            Vec2 direction = ChooseEscapeDirection();
            if (direction.SqrLength > 0f)
            {
                Vec2 ignored = Vec2.Zero;
                _player.Position = _terrain.Move(_player.Position, direction * (_player.RunSpeed * dt), ref ignored);
                _player.MoveDirection = direction;
            }

            if (_options.facing == SimFacingMode.NearestEnemy)
            {
                EnemyAgent nearest = FindNearestThreat();
                if (nearest != null)
                {
                    Vec2 toNearest = nearest.Position - _player.Position;
                    if (toNearest.SqrLength > 0.0001f)
                    {
                        _player.Facing = toNearest.Normalized;
                    }
                }
            }
            else
            {
                _player.Facing = _player.MoveDirection;
            }
        }

        private Vec2 ChooseEscapeDirection()
        {
            Array.Clear(_dangerBins, 0, _dangerBins.Length);
            float total = 0f;
            float radiusSqr = _options.escapeRadius * _options.escapeRadius;
            foreach (EnemyAgent enemy in _enemies)
            {
                if (enemy.Presence != Presence.Active || !enemy.IsThreat)
                {
                    continue;
                }

                Vec2 offset = enemy.Position - _player.Position;
                float sqrDistance = offset.SqrLength;
                if (sqrDistance > radiusSqr || sqrDistance < 1e-8f)
                {
                    continue;
                }

                float weight = 1f / MathF.Max(MathF.Sqrt(sqrDistance), 1f);
                _dangerBins[DirectionBin(offset)] += weight;
                total += weight;
            }

            Vec2 best = Vec2.Zero;
            float bestScore = float.PositiveInfinity;
            for (int k = 0; k < EscapeDirections; k++)
            {
                Vec2 direction = _escapeDirections[k];
                float free = _terrain.FreeDistance(_player.Position, direction, _options.escapeLookahead);
                if (free < _terrain.CellSize)
                {
                    continue;
                }

                float danger = 0f;
                if (total > 0f)
                {
                    for (int b = 0; b < EscapeDirections; b++)
                    {
                        if (_dangerBins[b] > 0f)
                        {
                            danger += _dangerBins[b] * _binWeights[(b - k + EscapeDirections) % EscapeDirections];
                        }
                    }
                    danger /= total;
                }

                float deadEnd = 1f - free / _options.escapeLookahead;
                float turn = (1f - Vec2.Dot(direction, _player.MoveDirection)) * 0.5f;
                float score = danger + _options.escapeDeadEndWeight * deadEnd + _options.escapeTurnWeight * turn;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = direction;
                }
            }
            return best;
        }

        private static int DirectionBin(Vec2 offset)
        {
            float angle = MathF.Atan2(offset.X, offset.Z) * (180f / MathF.PI);
            if (angle < 0f)
            {
                angle += 360f;
            }
            return (int)MathF.Round(angle / (360f / EscapeDirections)) % EscapeDirections;
        }

        private EnemyAgent FindNearestThreat()
        {
            EnemyAgent nearest = null;
            float minSqrDistance = float.PositiveInfinity;
            foreach (EnemyAgent enemy in _enemies)
            {
                if (enemy.Presence != Presence.Active || !enemy.IsThreat)
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

        private void UpdateEnemy(EnemyAgent enemy, float time, float dt)
        {
            if (enemy.Presence != Presence.Active)
            {
                return;
            }

            SimEnemyDef def = enemy.Def;
            float distance = (_player.Position - enemy.Position).Length;
            if (enemy.InKnockback)
            {
                enemy.KnockbackRemaining -= dt;
                Vec2 ignored = Vec2.Zero;
                enemy.Position = _terrain.Move(enemy.Position, enemy.KnockbackVelocity * dt, ref ignored);
                enemy.KnockbackVelocity *= KnockbackDecay;
                if (enemy.KnockbackRemaining <= 0f)
                {
                    enemy.InKnockback = false;
                }
            }
            else if (!enemy.InCombat)
            {
                if (distance <= def.engageDistance)
                {
                    enemy.InCombat = true;
                }
                else
                {
                    Movement.Apply(enemy, def.chase, _player.Position, _player.Facing, dt, _terrain);
                }
            }
            else if (distance > def.engageDistance + 1f)
            {
                enemy.InCombat = false;
                enemy.AttackRunning = false;
            }
            else
            {
                Movement.Apply(enemy, def.combat, _player.Position, _player.Facing, dt, _terrain);
                TryAttack(enemy, time);
            }

            Movement.Integrate(enemy, def.linearDamping, def.useGravity, dt, _terrain);
            UpdateAttack(enemy, time);
        }

        private void TryAttack(EnemyAgent enemy, float time)
        {
            SimAttackDef attack = enemy.Def.attack;
            if (attack.shotCount <= 0 || enemy.AttackRunning || time < enemy.NextAttackTime)
            {
                return;
            }

            enemy.NextAttackTime = time + attack.attackInterval;
            enemy.AttackRunning = true;
            enemy.ShotsFired = 0;
            enemy.NextShotTime = time + attack.interval * attack.firstShotRate + attack.interval;
        }

        private void UpdateAttack(EnemyAgent enemy, float time)
        {
            if (!enemy.AttackRunning || time < enemy.NextShotTime)
            {
                return;
            }

            SimAttackDef attack = enemy.Def.attack;
            Fire(enemy, attack, time);
            enemy.ShotsFired++;
            if (enemy.ShotsFired >= attack.shotCount)
            {
                enemy.AttackRunning = false;
            }
            else
            {
                enemy.NextShotTime = time + attack.interval;
            }
        }

        private void Fire(EnemyAgent enemy, SimAttackDef attack, float time)
        {
            Vec2 shotPoint = enemy.Position + enemy.Facing.LocalToWorld(attack.shotPointX, attack.shotPointZ);
            Vec2 direction = (_player.Position - shotPoint).Normalized;
            if (direction.SqrLength == 0f)
            {
                direction = enemy.Facing;
            }

            SimShotDef shot = attack.shot;
            switch (shot.kind)
            {
                case SimShotKind.Projectile:
                    _shots.Add(new ShotAgent
                    {
                        Def = shot,
                        Position = shotPoint,
                        Velocity = direction * shot.speed,
                        Facing = direction,
                        Damage = attack.attackPower,
                        Hp = shot.hp,
                    });
                    break;
                case SimShotKind.Area:
                    SpawnArea(shot.area, shotPoint, direction, time);
                    break;
            }
        }

        private void SpawnArea(SimAreaDef def, Vec2 origin, Vec2 forward, float time)
        {
            _areas.Add(new AreaAgent
            {
                Def = def,
                Origin = origin,
                Forward = forward,
                ExpireTime = def.lifetime > 0f ? time + def.lifetime : float.PositiveInfinity,
            });
        }

        private void UpdateShots(float dt)
        {
            float maxDistanceSqr = _terrain.Period * _terrain.Period;
            foreach (ShotAgent shot in _shots)
            {
                if (!shot.Alive)
                {
                    continue;
                }

                SimShotDef def = shot.Def;
                if (def.hasBrain)
                {
                    float distance = (_player.Position - shot.Position).Length;
                    if (!shot.InCombat && distance <= def.engageDistance)
                    {
                        shot.InCombat = true;
                    }
                    else if (shot.InCombat && distance > def.engageDistance + 1f)
                    {
                        shot.InCombat = false;
                    }

                    Movement.Apply(shot, shot.InCombat ? def.combat : def.chase, _player.Position, _player.Facing, dt, _terrain);
                }

                if (def.linearDamping > 0f)
                {
                    shot.Velocity *= 1f / (1f + def.linearDamping * dt);
                }

                Vec2 from = shot.Position;
                Vec2 to = from + shot.Velocity * dt;
                if (SegmentHitsCircle(from, to, _player.Position, _player.Radius + def.radius))
                {
                    _player.TakeDamage(def.damageRange * shot.Damage);
                    shot.Alive = false;
                    continue;
                }

                if (HitsWall(from, to))
                {
                    shot.Alive = false;
                    continue;
                }

                shot.Position = to;
                if ((shot.Position - _player.Position).SqrLength > maxDistanceSqr)
                {
                    shot.Alive = false;
                }
            }
        }

        private bool HitsWall(Vec2 from, Vec2 to)
        {
            Vec2 delta = to - from;
            int steps = Math.Max(1, (int)MathF.Ceiling(delta.Length / _terrain.CellSize));
            for (int i = 1; i <= steps; i++)
            {
                if (_terrain.IsBlocked(from + delta * ((float)i / steps)))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool SegmentHitsCircle(Vec2 from, Vec2 to, Vec2 center, float radius)
        {
            Vec2 segment = to - from;
            float lengthSqr = segment.SqrLength;
            float t = lengthSqr > 0f ? Math.Clamp(Vec2.Dot(center - from, segment) / lengthSqr, 0f, 1f) : 0f;
            Vec2 closest = from + segment * t;
            return (center - closest).SqrLength < radius * radius;
        }

        private void UpdateAreas(float time)
        {
            for (int i = _areas.Count - 1; i >= 0; i--)
            {
                AreaAgent area = _areas[i];
                if (time >= area.ExpireTime)
                {
                    _areas.RemoveAt(i);
                    continue;
                }

                bool inside = AreaContainsPlayer(area);
                if (inside && !area.PlayerInside)
                {
                    _player.TakeDamage(area.Def.damage);
                }
                area.PlayerInside = inside;
            }
        }

        private bool AreaContainsPlayer(AreaAgent area)
        {
            SimAreaDef def = area.Def;
            Vec2 center = area.Origin + area.Forward.LocalToWorld(def.centerX, def.centerZ);
            Vec2 offset = _player.Position - center;
            float localX = Math.Clamp(Vec2.Dot(offset, area.Forward.Right), -def.halfX, def.halfX);
            float localZ = Math.Clamp(Vec2.Dot(offset, area.Forward), -def.halfZ, def.halfZ);
            Vec2 closest = center + area.Forward.LocalToWorld(localX, localZ);
            return (_player.Position - closest).SqrLength < _player.Radius * _player.Radius;
        }

        private void UpdatePickups(float time, float dt)
        {
            SimPickupDef rangeDef = null;
            float range = 0f;
            float idleDistanceSqr = 0f;
            foreach (PickupAgent pickup in _pickups)
            {
                if (!pickup.Alive)
                {
                    continue;
                }

                SimPickupDef def = pickup.Def;
                if (def != rangeDef)
                {
                    rangeDef = def;
                    range = def.baseRange + _progression.RangeBonusOf(def.rangeKey);
                    float idleDistance = MathF.Max(range, def.radius + _player.Radius) + 1f;
                    idleDistanceSqr = idleDistance * idleDistance;
                }

                if (!pickup.InCombat && def.chase.kind == SimMovementKind.Stop
                    && (_player.Position - pickup.Position).SqrLength > idleDistanceSqr)
                {
                    continue;
                }

                float distance = (_player.Position - pickup.Position).Length;
                if (!pickup.InCombat)
                {
                    if (distance <= range)
                    {
                        pickup.InCombat = true;
                    }
                    else
                    {
                        Movement.Apply(pickup, def.chase, _player.Position, _player.Facing, dt, _terrain);
                    }
                }
                else if (distance > range + 1f)
                {
                    pickup.InCombat = false;
                }
                else
                {
                    Movement.Apply(pickup, def.combat, _player.Position, _player.Facing, dt, _terrain);
                }

                float reach = def.radius + _player.Radius;
                if ((_player.Position - pickup.Position).SqrLength < reach * reach)
                {
                    pickup.Alive = false;
                    Collect(def, time);
                    rangeDef = null;
                }
            }
        }

        private void Collect(SimPickupDef def, float time)
        {
            switch (def.kind)
            {
                case SimPickupKind.Exp:
                    _progression.AddExp(def.amount, time);
                    break;
                case SimPickupKind.Heal:
                    _player.Heal((int)def.amount);
                    break;
                case SimPickupKind.SpeedUp:
                    _player.RunSpeed += def.amount;
                    _buffs.Add(new SpeedBuff { Remaining = def.duration, Amount = def.amount });
                    break;
            }
        }

        private void UpdateContacts(float time)
        {
            foreach (EnemyAgent enemy in _enemies)
            {
                if (enemy.Presence != Presence.Active)
                {
                    continue;
                }

                float reach = enemy.Def.radius + _player.Radius;
                bool touching = (enemy.Position - _player.Position).SqrLength < reach * reach;
                if (touching && !enemy.InContact)
                {
                    if (enemy.Def.touchDamage > 0)
                    {
                        _player.TakeDamage(enemy.Def.touchDamage);
                    }

                    if (enemy.Def.contactArea.enabled)
                    {
                        SpawnArea(enemy.Def.contactArea, enemy.Position, Vec2.Forward, time);
                        enemy.Presence = Presence.Released;
                        enemy.AttackRunning = false;
                    }
                }
                enemy.InContact = touching;
            }
        }

        public void DamageEnemy(EnemyAgent enemy, int damage, float knockback, float time)
        {
            if (enemy.Presence != Presence.Active || enemy.DeathTime >= 0f)
            {
                return;
            }

            int applied = enemy.Def.damageRange * damage;
            if (applied > 0)
            {
                enemy.Hp = Math.Max(enemy.Hp - applied, 0);
                if (enemy.Hp == 0)
                {
                    enemy.DeathTime = time;
                    if (enemy.Def.hasEnemyComponent)
                    {
                        _kills++;
                    }
                }
            }

            if (!enemy.Def.hasEnemyComponent)
            {
                return;
            }

            enemy.FinalDamage = applied;
            Vec2 away = (enemy.Position - _player.Position).Normalized;
            enemy.KnockbackVelocity = away * (knockback * (1f - enemy.Def.knockbackResistance));
            enemy.KnockbackRemaining = enemy.Def.knockbackDuration;
            enemy.InKnockback = true;
            enemy.InCombat = false;
            enemy.AttackRunning = false;
        }

        private void ProcessDeaths(float time)
        {
            foreach (EnemyAgent enemy in _enemies)
            {
                if (enemy.Presence != Presence.Active || enemy.DeathTime < 0f || time < enemy.DeathTime + enemy.Def.deathDelay)
                {
                    continue;
                }

                SpawnDrop(enemy);
                if (enemy.Def.hasEnemyComponent)
                {
                    SpawnDeathExplosion(enemy);
                    enemy.Generator.ActiveEnemies.Remove(enemy);
                }
                Release(enemy);
            }
        }

        private void SpawnDrop(EnemyAgent enemy)
        {
            List<SimDropEntry> drops = enemy.Def.drops;
            if (drops.Count == 0)
            {
                return;
            }

            int total = 0;
            foreach (SimDropEntry drop in drops)
            {
                total += drop.weight;
            }

            int random = total > 0 ? _random.Next(total) : 0;
            foreach (SimDropEntry drop in drops)
            {
                if (random < drop.weight)
                {
                    if (!string.IsNullOrEmpty(drop.pickupName) && _data.Pickups.TryGetValue(drop.pickupName, out SimPickupDef def))
                    {
                        _pickups.Add(new PickupAgent { Def = def, Position = enemy.Position });
                    }
                    return;
                }
                random -= drop.weight;
            }
        }

        private static void Release(EnemyAgent enemy)
        {
            enemy.Presence = Presence.Released;
            enemy.AttackRunning = false;
        }

        private void UpdateWindow()
        {
            int gridX = _terrain.WorldToGrid(_player.Position.X);
            int gridZ = _terrain.WorldToGrid(_player.Position.Z);
            if (gridX == _gridX && gridZ == _gridZ)
            {
                return;
            }

            _gridX = gridX;
            _gridZ = gridZ;

            foreach (EnemyAgent enemy in _enemies)
            {
                if (enemy.Presence != Presence.Active)
                {
                    continue;
                }

                float shiftX = WindowShift(enemy.Position.X, gridX);
                float shiftZ = WindowShift(enemy.Position.Z, gridZ);
                if (shiftX == 0f && shiftZ == 0f)
                {
                    continue;
                }

                if (enemy.Def.hasEnemyComponent)
                {
                    enemy.Position += new Vec2(shiftX, shiftZ);
                    enemy.Velocity = Vec2.Zero;
                }
                else
                {
                    enemy.Presence = Presence.Fallen;
                    enemy.AttackRunning = false;
                }
            }

            foreach (PickupAgent pickup in _pickups)
            {
                if (pickup.Alive && (WindowShift(pickup.Position.X, gridX) != 0f || WindowShift(pickup.Position.Z, gridZ) != 0f))
                {
                    pickup.Alive = false;
                }
            }
        }

        private float WindowShift(float coord, int playerGrid)
        {
            int half = _terrain.GridSize / 2;
            int grid = _terrain.WorldToGrid(coord);
            float shift = 0f;
            while (grid + (int)MathF.Round(shift / _terrain.TileSize) < playerGrid - half)
            {
                shift += _terrain.Period;
            }
            while (grid + (int)MathF.Round(shift / _terrain.TileSize) > playerGrid + half)
            {
                shift -= _terrain.Period;
            }
            return shift;
        }

        private void Compact()
        {
            _enemies.RemoveAll(e => e.Presence != Presence.Active);
            _shots.RemoveAll(s => !s.Alive);
            _pickups.RemoveAll(p => !p.Alive);
            _projectiles.RemoveAll(p => !p.Alive);
        }

        private void Record(float time)
        {
            int sampleCount = _samples[0].Length;
            while (_nextSample < sampleCount && _nextSample * _options.sampleInterval <= time + 1e-4f)
            {
                WriteSample(_nextSample, _player.DeathTriggered ? 0f : 1f);
                _nextSample++;
            }
        }

        private void FillRemainingSamples(bool cleared)
        {
            int sampleCount = _samples[0].Length;
            while (_nextSample < sampleCount)
            {
                WriteSample(_nextSample, cleared ? 1f : 0f);
                _nextSample++;
            }
        }

        private void WriteSample(int index, float alive)
        {
            _samples[0][index] = _progression.Level;
            _samples[1][index] = _kills;
            _samples[2][index] = _player.Hp;
            _samples[3][index] = alive;

            for (int g = 0; g < _generators.Count; g++)
            {
                GeneratorState generator = _generators[g];
                int inWorld = 0;
                foreach (EnemyAgent enemy in _enemies)
                {
                    if (enemy.Generator == generator && enemy.Presence == Presence.Active)
                    {
                        inWorld++;
                    }
                }
                _samples[FixedSeriesCount + g * 2][index] = inWorld;
                _samples[FixedSeriesCount + g * 2 + 1][index] = generator.ActiveEnemies.Count;
            }

            int evolutionOffset = FixedSeriesCount + _generators.Count * 2;
            for (int e = 0; e < _evolvedWeapons.Count; e++)
            {
                _samples[evolutionOffset + e][index] = _progression.HasEvolved(_evolvedWeapons[e]) ? 1f : 0f;
            }
        }
    }
}
