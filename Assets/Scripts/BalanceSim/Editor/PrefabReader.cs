using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    public sealed class PrefabReader
    {
        private readonly HashSet<EnemyConfig> _initializedConfigs;
        private readonly ProjectileReader _projectiles;
        private readonly List<string> _warnings;
        private readonly Dictionary<string, GameObject> _enemySources = new();
        private readonly Dictionary<string, GameObject> _pickupSources = new();

        public List<SimEnemyDef> Enemies { get; } = new();
        public List<SimPickupDef> Pickups { get; } = new();

        public PrefabReader(HashSet<EnemyConfig> initializedConfigs, ProjectileReader projectiles, List<string> warnings)
        {
            _initializedConfigs = initializedConfigs;
            _projectiles = projectiles;
            _warnings = warnings;
        }

        public string AddEnemy(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            if (_enemySources.TryGetValue(prefab.name, out GameObject known))
            {
                if (known != prefab)
                {
                    _warnings.Add($"同じ名前の敵プレハブが複数あります: {prefab.name}");
                }
                return prefab.name;
            }

            _enemySources[prefab.name] = prefab;
            Enemies.Add(ReadEnemy(prefab));
            return prefab.name;
        }

        private SimEnemyDef ReadEnemy(GameObject prefab)
        {
            var enemy = prefab.GetComponent<Enemy>();
            var projectile = prefab.GetComponent<enemyplojectileobject>();
            var brain = prefab.GetComponent<EnemyBrain>();
            UnityEngine.Object owner = enemy != null ? enemy : projectile;

            EnemyConfig config = brain != null ? brain.Config : null;
            if (config == null && owner != null)
            {
                config = SerializedReader.Ref<EnemyConfig>(owner, "config");
            }

            var def = new SimEnemyDef
            {
                name = prefab.name,
                hasEnemyComponent = enemy != null,
                radius = SerializedReader.HorizontalRadius(prefab, _ => true),
            };

            if (owner == null)
            {
                _warnings.Add($"{prefab.name} に Enemy も enemyplojectileobject もありません。動かない的として扱います");
                return def;
            }

            if (config != null)
            {
                def.maxHp = config.MaxHp;
                def.damageRange = config.Damagelange;
                def.knockbackResistance = config.KnockbackResistance;
                def.knockbackDuration = config.KnockbackDuration;
                def.engageDistance = EngageDistance(config);
                def.chase = ReadMovement(config.chaseMovedata);
                def.combat = ReadMovement(config.combatMovedata);
            }
            else
            {
                _warnings.Add($"{prefab.name} の EnemyConfig がありません");
            }

            if (prefab.TryGetComponent(out Rigidbody body))
            {
                def.linearDamping = body.linearDamping;
                def.useGravity = body.useGravity && !body.isKinematic;
            }

            var touch = prefab.GetComponentInChildren<ToutchDamageController>(true);
            if (touch != null)
            {
                def.touchDamage = SerializedReader.Int(touch, "damageAmount");
            }

            var attackController = prefab.GetComponent<EnemyAttackController>();
            if (attackController != null && config != null)
            {
                def.attack = ReadAttack(prefab, config, attackController);
            }

            if (projectile is ballprojectile)
            {
                var area = SerializedReader.Ref<GameObject>(projectile, "prefab");
                def.contactArea = area != null ? ReadArea(area) : new SimAreaDef();
                def.contactArea.enabled = true;
            }
            else if (projectile != null)
            {
                _warnings.Add($"{prefab.name} は {projectile.GetType().Name} で出現します。接触時の処理は写していません");
            }

            def.deathDelay = SerializedReader.Float(owner, "deathDelay");
            if (enemy != null)
            {
                def.destroyDamageDamping = SerializedReader.Float(enemy, "destroyDamageDamping");
                def.deathExplosion = _projectiles.Add(SerializedReader.Ref<GameObject>(enemy, "DestroyDamagePrefab"));
            }

            SerializedProperty drops = SerializedReader.Prop(owner, "dropList");
            for (int i = 0; i < drops.arraySize; i++)
            {
                SerializedProperty drop = drops.GetArrayElementAtIndex(i);
                var dropPrefab = drop.FindPropertyRelative("prefab").objectReferenceValue as GameObject;
                def.drops.Add(new SimDropEntry
                {
                    pickupName = AddPickup(dropPrefab) ?? string.Empty,
                    weight = drop.FindPropertyRelative("weight").intValue,
                });
            }

            return def;
        }

        private SimAttackDef ReadAttack(GameObject prefab, EnemyConfig config, EnemyAttackController attackController)
        {
            var attack = new SimAttackDef();
            if (config.AttackPattern == null)
            {
                return attack;
            }

            if (config.AttackPattern is not BurstShotPattern pattern)
            {
                _warnings.Add($"{prefab.name} の攻撃パターン {config.AttackPattern.GetType().Name} は未対応です。攻撃しないものとして扱います");
                return attack;
            }

            var launcher = SerializedReader.Ref<ProjectileLauncher>(attackController, "launcher");
            var shotPoint = SerializedReader.Ref<Transform>(attackController, "shotPoint");
            if (launcher == null || shotPoint == null)
            {
                return attack;
            }

            var bulletData = SerializedReader.Ref<BulletData>(launcher, "bulletData");
            if (bulletData == null || bulletData.AmmpPrefab == null)
            {
                return attack;
            }

            Vector3 offset = Quaternion.Inverse(prefab.transform.rotation) * (shotPoint.position - prefab.transform.position);
            attack.shotCount = SerializedReader.Int(pattern, "shotCount");
            attack.interval = pattern.Interval;
            attack.firstShotRate = pattern.FirstshotInterval;
            attack.attackInterval = config.AttackInterval;
            attack.attackPower = config.AttackPower;
            attack.shotPointX = offset.x;
            attack.shotPointZ = offset.z;
            attack.shot = ReadShot(prefab, bulletData);
            if (attack.shot.kind == SimShotKind.None)
            {
                attack.shotCount = 0;
            }
            return attack;
        }

        private SimShotDef ReadShot(GameObject owner, BulletData bulletData)
        {
            GameObject ammo = bulletData.AmmpPrefab;
            var shot = new SimShotDef();
            var projectile = ammo.GetComponent<enemyplojectileobject>();
            if (projectile != null)
            {
                if (projectile is not basicenemyprojectile)
                {
                    _warnings.Add($"{owner.name} の弾 {ammo.name} は {projectile.GetType().Name} です。通常の弾として扱います");
                }

                var config = SerializedReader.Ref<EnemyConfig>(projectile, "config");
                shot.kind = SimShotKind.Projectile;
                shot.speed = bulletData.Speed;
                shot.radius = SerializedReader.HorizontalRadius(ammo, _ => true);
                shot.damageRange = config != null ? config.Damagelange : 1;
                shot.hp = config != null ? config.MaxHp : 1;

                var brain = ammo.GetComponent<EnemyBrain>();
                if (brain != null && brain.Config != null)
                {
                    shot.hasBrain = true;
                    shot.engageDistance = EngageDistance(brain.Config);
                    shot.chase = ReadMovement(brain.Config.chaseMovedata);
                    shot.combat = ReadMovement(brain.Config.combatMovedata);
                }

                if (ammo.TryGetComponent(out Rigidbody body))
                {
                    shot.linearDamping = body.linearDamping;
                }
                return shot;
            }

            if (ammo.GetComponent<DamageArea>() != null)
            {
                shot.kind = SimShotKind.Area;
                shot.area = ReadArea(ammo);
                return shot;
            }

            _warnings.Add($"{owner.name} の弾 {ammo.name} の種類が分かりません。攻撃しないものとして扱います");
            return shot;
        }

        private SimAreaDef ReadArea(GameObject prefab)
        {
            var area = prefab.GetComponent<DamageArea>();
            if (area == null)
            {
                _warnings.Add($"{prefab.name} に DamageArea がありません");
                return new SimAreaDef();
            }

            if (SerializedReader.Int(area, "mode") != (int)DamageAreaMode.OnEnterOnce)
            {
                _warnings.Add($"{prefab.name} は Interval モードです。入った瞬間の1回だけとして扱います");
            }

            bool targetsPlayer = (SerializedReader.Int(area, "targetTeam") & (int)TeamType.Player) != 0;
            Bounds bounds = SerializedReader.LocalBounds(prefab, c => c.isTrigger);
            return new SimAreaDef
            {
                enabled = true,
                damage = targetsPlayer ? SerializedReader.Int(area, "damage") : 0,
                lifetime = SerializedReader.Float(area, "lifetime"),
                centerX = bounds.center.x,
                centerZ = bounds.center.z,
                halfX = bounds.extents.x,
                halfZ = bounds.extents.z,
            };
        }

        private string AddPickup(GameObject prefab)
        {
            if (prefab == null)
            {
                return null;
            }

            if (_pickupSources.TryGetValue(prefab.name, out GameObject known))
            {
                if (known != prefab)
                {
                    _warnings.Add($"同じ名前のドロッププレハブが複数あります: {prefab.name}");
                }
                return prefab.name;
            }

            var item = prefab.GetComponent<PickupTriggerItem>();
            if (item == null)
            {
                _warnings.Add($"ドロップ {prefab.name} は拾えるアイテムではありません。何も落とさないものとして扱います");
                return null;
            }

            _pickupSources[prefab.name] = prefab;
            var def = new SimPickupDef
            {
                name = prefab.name,
                radius = SerializedReader.HorizontalRadius(prefab, _ => true),
            };

            switch (item)
            {
                case Exp exp:
                    def.kind = SimPickupKind.Exp;
                    def.amount = exp.ExpAmount;
                    break;
                case healingHpitem heal:
                    def.kind = SimPickupKind.Heal;
                    def.amount = heal.HealAmount;
                    break;
                case speedupitem speed:
                    def.kind = SimPickupKind.SpeedUp;
                    def.amount = SerializedReader.Float(speed, "upspeedrate");
                    def.duration = SerializedReader.Float(speed, "duration");
                    break;
                default:
                    _warnings.Add($"ドロップ {prefab.name} の種類 {item.GetType().Name} は未対応です。何も落とさないものとして扱います");
                    return null;
            }

            var brain = SerializedReader.Ref<EnemyBrain>(item, "enemyBrain");
            if (brain != null && brain.Config != null)
            {
                def.rangeKey = brain.Config.name;
                def.baseRange = EngageDistance(brain.Config);
                def.chase = ReadMovement(brain.Config.chaseMovedata);
                def.combat = ReadMovement(brain.Config.combatMovedata);
            }

            Pickups.Add(def);
            return prefab.name;
        }

        private float EngageDistance(EnemyConfig config)
        {
            return _initializedConfigs.Contains(config) ? SerializedReader.Float(config, "engagedistance") : 0f;
        }

        private SimMovementDef ReadMovement(MovementPattern pattern)
        {
            var move = new SimMovementDef();
            switch (pattern)
            {
                case null:
                case stop:
                    move.kind = SimMovementKind.Stop;
                    break;
                case TowardMovement toward:
                    move.kind = SimMovementKind.Toward;
                    move.acceleration = SerializedReader.Float(toward, "acceleration");
                    move.maxSpeed = SerializedReader.Float(toward, "maxSpeed");
                    break;
                case ItemMovement item:
                    move.kind = SimMovementKind.Item;
                    move.chaseSpeed = item.ChaseSpeed;
                    break;
                case OrbitMovement orbit:
                    move.kind = SimMovementKind.Orbit;
                    move.orbitRadius = orbit.OrbitRadius;
                    move.orbitAngularSpeed = orbit.OrbitAngularSpeed;
                    break;
                case RollMovement roll:
                    move.kind = SimMovementKind.Roll;
                    move.moveForce = roll.MoveForce;
                    break;
                case JumpMovement jump:
                    move.kind = SimMovementKind.Jump;
                    move.forwardForce = jump.ForwardForce;
                    move.upwardForce = jump.UpwardForce;
                    break;
                default:
                    _warnings.Add($"移動パターン {pattern.GetType().Name}（{pattern.name}）は未対応です。止まっているものとして扱います");
                    move.kind = SimMovementKind.Stop;
                    break;
            }
            return move;
        }
    }
}
