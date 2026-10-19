using System;
using System.Collections.Generic;
using System.Linq;
using InfiniteTileWorld;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BalanceSim.Editor
{
    /// <summary>シーンの設定をバランス検証用の入力データへ変換する</summary>
    public static class SimInputBuilder
    {
        private static readonly string[] InventorySlotFields =
        {
            "leftWeapon", "rightWeapon", "leftAbility", "rightAbility", "leftAutoWeapon", "rightAutoWeapon",
        };

        /// <summary>対象シーンと各調整ケースからシミュレーション入力を構築する</summary>
        public static void Build(BalanceSimSettings settings, IReadOnlyList<SimValueCase> cases, Action<int, SimInput> onBuilt,
            List<string> warnings, out string terrainSummary)
        {
            string scenePath = AssetDatabase.GetAssetPath(settings.TargetScene);
            if (string.IsNullOrEmpty(scenePath))
            {
                throw new InvalidOperationException("BalanceSimSettings の対象シーンが設定されていません");
            }

            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedHere = !scene.isLoaded;
            if (openedHere)
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }

            try
            {
                bool sceneWasDirty = scene.isDirty;
                terrainSummary = null;
                var terrainCache = new TerrainSampler.Cache();
                for (int i = 0; i < cases.Count; i++)
                {
                    if (cases.Count > 1 && EditorUtility.DisplayCancelableProgressBar("BalanceSim", $"値の読み出し {i + 1}/{cases.Count}", (float)i / cases.Count))
                    {
                        throw new OperationCanceledException();
                    }

                    SimInput input;
                    using (SimValueApplier.Apply(cases[i].Rows, scene, warnings))
                    {
                        input = Build(settings, scene, terrainCache, warnings, out terrainSummary);
                    }
                    onBuilt(i, input);
                }
                if (!openedHere && !sceneWasDirty && scene.isDirty)
                {
                    warnings.Add($"数値SO の上書きで、開いているシーン {scene.name} に変更ありの印が付きました。値は実行前に戻しています");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (openedHere)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        /// <summary>1ケース分のプレイヤー、敵、地形、装備の設定を読み取る</summary>
        private static SimInput Build(BalanceSimSettings settings, Scene scene, TerrainSampler.Cache terrainCache, List<string> warnings, out string terrainSummary)
        {
            var input = new SimInput
            {
                runCount = settings.RunCount,
                seed = settings.Seed,
                options = new SimOptions
                {
                    timeStep = settings.TimeStep,
                    sampleInterval = settings.KeepRunDetails ? settings.SampleInterval : settings.SummarySampleInterval,
                    keepRunDetails = settings.KeepRunDetails,
                    facing = settings.Facing,
                    escapeRadius = settings.EscapeRadius,
                    escapeLookahead = settings.EscapeLookahead,
                    escapeDeadEndWeight = settings.EscapeDeadEndWeight,
                    escapeTurnWeight = settings.EscapeTurnWeight,
                    aimError = settings.AimError,
                    fireRange = settings.FireRange,
                },
                choicePolicy = new SimPolicyRef
                {
                    typeName = settings.ChoicePolicy?.GetType().FullName,
                    json = settings.ChoicePolicy != null ? JsonUtility.ToJson(settings.ChoicePolicy) : null,
                },
            };

            Player player = Single<Player>(scene);
            input.timeLimit = Single<Timer>(scene).TimeLimit;
            var projectiles = new ProjectileReader(warnings);
            var weapons = new WeaponCollector(input.weapons, projectiles, warnings);
            ReadPlayer(scene, player, input, weapons, warnings);

            var pickupMagnet = player.GetComponent<PickupMagnet>();
            if (pickupMagnet == null)
            {
                warnings.Add("プレイヤーに PickupMagnet がありません。アイテムは引き寄せられないものとして扱います");
            }
            var prefabs = new PrefabReader(projectiles, warnings, player.Config.Stats.PickupRadius, pickupMagnet);
            foreach (EnemyGenerator generator in FindActive<EnemyGenerator>(scene))
            {
                input.generators.Add(ReadGenerator(generator, player, prefabs, warnings));
            }
            if (input.generators.Count == 0)
            {
                warnings.Add("有効な EnemyGenerator がシーンにありません");
            }
            input.enemies = prefabs.Enemies;
            input.pickups = prefabs.Pickups;

            ReadUpgrades(Single<UpgradeManager>(scene), input, weapons, warnings);
            input.projectiles = projectiles.Projectiles;

            input.terrain = TerrainSampler.Sample(Single<StageManager>(scene), player, settings.TerrainCellSize, terrainCache, warnings, out float blockedRatio);
            terrainSummary = $"地形 {input.terrain.gridSize}×{input.terrain.gridSize} 枚（一辺 {input.terrain.tileSize * input.terrain.gridSize}m）、格子 {input.terrain.cells}×{input.terrain.cells}、通れないマス {blockedRatio:P1}";
            return input;
        }

        /// <summary>共有設定SOから開始時のプレイヤー能力と成長値を読み取る</summary>
        private static void ReadPlayer(Scene scene, Player player, SimInput input, WeaponCollector weapons, List<string> warnings)
        {
            PlayerConfig config = player.Config;
            if (config == null) throw new InvalidOperationException("プレイヤー設定の入口がありません");
            if (!config.Validate(out string configurationError)) throw new InvalidOperationException(configurationError);
            var controller = player.GetComponent<CharacterController>();
            if (controller == null)
            {
                warnings.Add("プレイヤーに CharacterController がありません。半径を 0.5 とします");
            }

            Vector3 position = player.transform.position;
            Vector3 scale = player.transform.lossyScale;
            float footY = player.transform.TransformPoint(config.Collision.Center).y
                - config.Collision.Height * 0.5f * Mathf.Abs(scale.y);
            Camera camera = FindActive<Camera>(scene).FirstOrDefault(c => c.CompareTag("MainCamera"));
            float eyeHeight;
            if (camera == null)
            {
                warnings.Add("MainCamera がシーンにありません。目の高さを 1.6m とします");
                eyeHeight = 1.6f;
            }
            else
            {
                Vector3 cameraPosition = camera.transform.position;
                if (new Vector2(cameraPosition.x - position.x, cameraPosition.z - position.z).sqrMagnitude > 1f)
                {
                    warnings.Add("MainCamera がプレイヤーから水平に 1m 以上離れています。弾はプレイヤーの位置から出るものとして扱います");
                }
                eyeHeight = cameraPosition.y - footY;
            }


            input.player = new SimPlayerDef
            {
                x = position.x,
                z = position.z,
                radius = config.Collision.Radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)),
                maxHp = config.Stats.InitialHP,
                runSpeed = config.Stats.RunSpeed,
                levelUpRequiredExp = config.Progression.LevelUpRequiredExp,
                levelUpRate = config.Progression.LevelUpRate,
                eyeHeight = eyeHeight,
                pivotHeight = position.y - footY,
            };

            foreach (string field in InventorySlotFields)
            {
                var weapon = SerializedReader.Ref<Weapon>(player.Inventory, field);
                WeaponData data = weapon != null ? weapon.WeaponData : null;
                weapons.Add(data);
                input.player.initialWeapons.Add(data != null && !data.IsEmpty ? data.name : string.Empty);
            }
        }

        /// <summary>敵生成器の設定とフェーズをシミュレーション用に変換する</summary>
        private static SimGeneratorDef ReadGenerator(EnemyGenerator generator, Player player, PrefabReader prefabs, List<string> warnings)
        {
            Vector3 offset = generator.transform.position - player.transform.position;
            if (new Vector2(offset.x, offset.z).sqrMagnitude > 0.01f)
            {
                warnings.Add($"{generator.name} がプレイヤーの位置にありません。プレイヤーの位置から出現するものとして扱います");
            }

            var def = new SimGeneratorDef
            {
                name = generator.gameObject.name,
                spawnRadius = SerializedReader.Float(generator, "spawnRadius"),
                maxSpawnTry = SerializedReader.Int(generator, "maxSpawnTry"),
                maxEnemyCount = SerializedReader.Int(generator, "maxEnemyCount"),
                relocateDistance = SerializedReader.Float(generator, "relocateDistance"),
            };

            SerializedProperty directions = SerializedReader.Prop(generator, "spawnDirections");
            for (int i = 0; i < directions.arraySize; i++)
            {
                SerializedProperty direction = directions.GetArrayElementAtIndex(i);
                def.directions.Add(new SimSpawnDirection
                {
                    angle = direction.FindPropertyRelative("angle").floatValue,
                    weight = direction.FindPropertyRelative("weight").floatValue,
                });
            }

            SerializedProperty phases = SerializedReader.Prop(generator, "phases");
            for (int i = 0; i < phases.arraySize; i++)
            {
                if (phases.GetArrayElementAtIndex(i).objectReferenceValue is PhaseData phase)
                {
                    def.phases.Add(ToSimPhase(phase, prefabs, warnings));
                }
            }
            return def;
        }

        /// <summary>敵の生成フェーズを設定済みPrefabの参照とともに変換する</summary>
        private static SimPhase ToSimPhase(PhaseData phase, PrefabReader prefabs, List<string> warnings)
        {
            var simPhase = new SimPhase
            {
                name = phase.name,
                startTime = phase.StartTime,
                minSpawnCount = phase.MinSpawnCount,
                spawnInterval = phase.SpawnInterval,
            };

            if (phase.Enemies == null)
            {
                return simPhase;
            }

            foreach (EnemySpawnData entry in phase.Enemies)
            {
                if (entry.Prefab == null)
                {
                    warnings.Add($"{phase.name} に Prefab が空の出現設定があります");
                }

                simPhase.enemies.Add(new SimSpawnEntry
                {
                    prefabName = prefabs.AddEnemy(entry.Prefab),
                    spawnWeight = entry.SpawnWeight,
                });
            }
            return simPhase;
        }

        /// <summary>強化候補の武器とアイテムをシミュレーション用に登録する</summary>
        private static void ReadUpgrades(UpgradeManager manager, SimInput input, WeaponCollector weapons, List<string> warnings)
        {
            var items = new ItemCollector(input.items, warnings);
            SerializedProperty pool = SerializedReader.Prop(manager, "upgradePool");
            for (int i = 0; i < pool.arraySize; i++)
            {
                var upgrade = pool.GetArrayElementAtIndex(i).objectReferenceValue as UpgradeBase;
                if (upgrade == null)
                {
                    continue;
                }

                var def = new SimUpgradeDef { name = upgrade.name, weight = upgrade.Weight };
                switch (upgrade)
                {
                    case LevelUp:
                        def.kind = SimUpgradeKind.WeaponLevelUp;
                        def.slot = SerializedReader.Int(upgrade, "targetType");
                        break;
                    case Unlock:
                    {
                        def.kind = SimUpgradeKind.WeaponUnlock;
                        var target = SerializedReader.Ref<WeaponData>(upgrade, "target");
                        weapons.Add(target);
                        def.weapon = target != null && !target.IsEmpty ? target.name : string.Empty;
                        break;
                    }
                    case GetItem:
                    {
                        def.kind = SimUpgradeKind.GetItem;
                        var target = SerializedReader.Ref<Item>(upgrade, "target");
                        items.Add(target);
                        def.item = target != null ? target.name : string.Empty;
                        break;
                    }
                    case ItemLevelUp:
                        def.kind = SimUpgradeKind.ItemLevelUp;
                        def.itemIndex = SerializedReader.Int(upgrade, "itemIndex");
                        break;
                    default:
                        warnings.Add($"強化 {upgrade.name} の種類 {upgrade.GetType().Name} は未対応です。候補から外します");
                        continue;
                }
                input.upgrades.Add(def);
            }
        }

        /// <summary>シーン内の有効な対象が1つだけであることを確認して取得する</summary>
        private static T Single<T>(Scene scene) where T : Component
        {
            List<T> found = FindActive<T>(scene);
            if (found.Count == 0)
            {
                throw new InvalidOperationException($"{scene.name} に有効な {typeof(T).Name} がありません");
            }
            if (found.Count > 1)
            {
                throw new InvalidOperationException($"{scene.name} に有効な {typeof(T).Name} が {found.Count} 個あります");
            }
            return found[0];
        }

        /// <summary>対象シーンに属する有効なコンポーネントを列挙する</summary>
        private static List<T> FindActive<T>(Scene scene) where T : Component
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .Where(c => c.gameObject.activeInHierarchy && (c is not Behaviour behaviour || behaviour.enabled))
                .ToList();
        }

        /// <summary>武器と強化先を重複させずにシミュレーション入力へ集約する</summary>
        private sealed class WeaponCollector
        {
            private readonly List<SimWeaponDef> _weapons;
            private readonly ProjectileReader _projectiles;
            private readonly List<string> _warnings;
            private readonly Dictionary<string, WeaponData> _known = new();

            /// <summary>武器一覧、弾の読取器、警告の保存先を関連付ける</summary>
            public WeaponCollector(List<SimWeaponDef> weapons, ProjectileReader projectiles, List<string> warnings)
            {
                _weapons = weapons;
                _projectiles = projectiles;
                _warnings = warnings;
            }

            /// <summary>武器とその強化先を一覧へ登録する</summary>
            public void Add(WeaponData data)
            {
                while (data != null && !data.IsEmpty)
                {
                    if (_known.TryGetValue(data.name, out WeaponData known))
                    {
                        if (known != data)
                        {
                            _warnings.Add($"同じ名前の武器データが複数あります: {data.name}");
                        }
                        return;
                    }

                    _known[data.name] = data;
                    WeaponData next = data.NextLevelData;
                    _weapons.Add(new SimWeaponDef
                    {
                        name = data.name,
                        type = (SimWeaponType)(int)data.WeaponType,
                        next = next != null && !next.IsEmpty ? next.name : string.Empty,
                        autoFire = data.AutoFire,
                        autoReload = data.AutoReload,
                        fullAuto = data.TriggerType == WeaponTriggerType.FullAuto,
                        damage = data.Damage,
                        knockback = data.KnockbackForce,
                        spreadAngle = data.SpreadAngle,
                        fireInterval = data.FireInterval,
                        burstCount = data.BurstCount,
                        burstInterval = data.BurstInterval,
                        magazineSize = data.MagazineSize,
                        reloadTime = data.ReloadTime,
                        fireMode = ReadFireMode(data),
                    });
                    data = next;
                }
            }

            /// <summary>武器の攻撃方式をシミュレーションの攻撃定義へ変換する</summary>
            private SimFireModeDef ReadFireMode(WeaponData data)
            {
                FireModeData mode = data.FireModeData;
                var def = new SimFireModeDef { name = mode != null ? mode.name : string.Empty };
                switch (mode)
                {
                    case null:
                        _warnings.Add($"武器 {data.name} に射撃方式がありません。撃っても何も起きないものとして扱います");
                        break;
                    case ChainFire chain:
                        def.kind = SimFireModeKind.Chain;
                        def.maxRange = SerializedReader.Float(chain, "maxRange");
                        def.hitRadius = SerializedReader.Float(chain, "hitRadius");
                        def.targetTeam = SerializedReader.Int(chain, "targetTeam");
                        def.chainRange = SerializedReader.Float(chain, "chainRange");
                        def.chainCount = SerializedReader.Int(chain, "chainCount");
                        break;
                    case HitScanData hitScan:
                        def.kind = SimFireModeKind.HitScan;
                        def.maxRange = SerializedReader.Float(hitScan, "maxRange");
                        def.hitRadius = SerializedReader.Float(hitScan, "hitRadius");
                        def.targetTeam = SerializedReader.Int(hitScan, "targetTeam");
                        break;
                    case NormalProjectileFire projectile:
                        def.kind = SimFireModeKind.Projectile;
                        ReadPrefabAndOffset(projectile, def, "Offset");
                        def.tracking = SerializedReader.Prop(projectile, "isTracking").boolValue;
                        break;
                    case SurroundedFire surrounded:
                        def.kind = SimFireModeKind.Surrounded;
                        ReadPrefabAndOffset(surrounded, def, "Offset");
                        def.tracking = SerializedReader.Prop(surrounded, "isTracking").boolValue;
                        def.projectileCount = SerializedReader.Int(surrounded, "projectileCount");
                        break;
                    case AreaFireData area:
                    {
                        def.kind = SimFireModeKind.Area;
                        def.projectile = _projectiles.Add(SerializedReader.Ref<GameObject>(area, "prefab"));
                        Vector2 size = SerializedReader.Prop(area, "areaSize").vector2Value;
                        def.areaSizeX = size.x;
                        def.areaSizeZ = size.y;
                        def.spawnHeight = SerializedReader.Float(area, "spawnHeight");
                        def.projectileCount = SerializedReader.Int(area, "projectileCount");
                        break;
                    }
                    case AutoTargetFire autoTarget:
                        def.kind = SimFireModeKind.AutoTarget;
                        ReadPrefabAndOffset(autoTarget, def, "spawnOffset");
                        def.searchRadius = SerializedReader.Float(autoTarget, "searchRadius");
                        break;
                    case StepFire step:
                    {
                        def.kind = SimFireModeKind.Step;
                        def.projectile = _projectiles.Add(SerializedReader.Ref<GameObject>(step, "prefab"), upward: true);
                        Vector3 offset = Quaternion.LookRotation(Vector3.up) * SerializedReader.Prop(step, "Offset").vector3Value;
                        def.offsetX = offset.x;
                        def.offsetZ = offset.z;
                        def.stepInterval = SerializedReader.Float(step, "stepInterval");
                        break;
                    }
                    default:
                        _warnings.Add($"武器 {data.name} の射撃方式 {mode.GetType().Name} は未対応です。撃っても何も起きないものとして扱います");
                        break;
                }

                if (def.kind is not (SimFireModeKind.None or SimFireModeKind.HitScan or SimFireModeKind.Chain) && string.IsNullOrEmpty(def.projectile))
                {
                    _warnings.Add($"射撃方式 {def.name} に弾のプレハブがありません");
                }
                return def;
            }

            /// <summary>攻撃Prefabと発射位置の補正を読み取る</summary>
            private void ReadPrefabAndOffset(FireModeData mode, SimFireModeDef def, string offsetField)
            {
                def.projectile = _projectiles.Add(SerializedReader.Ref<GameObject>(mode, "prefab"));
                Vector3 offset = SerializedReader.Prop(mode, offsetField).vector3Value;
                def.offsetX = offset.x;
                def.offsetZ = offset.z;
            }
        }

        /// <summary>アイテムと強化先を重複させずにシミュレーション入力へ集約する</summary>
        private sealed class ItemCollector
        {
            private readonly List<SimItemDef> _items;
            private readonly List<string> _warnings;
            private readonly Dictionary<string, Item> _known = new();

            /// <summary>アイテム一覧と警告の保存先を関連付ける</summary>
            public ItemCollector(List<SimItemDef> items, List<string> warnings)
            {
                _items = items;
                _warnings = warnings;
            }

            /// <summary>アイテムとその強化先を一覧へ登録する</summary>
            public void Add(Item item)
            {
                while (item != null)
                {
                    if (_known.TryGetValue(item.name, out Item known))
                    {
                        if (known != item)
                        {
                            _warnings.Add($"同じ名前のアイテムが複数あります: {item.name}");
                        }
                        return;
                    }

                    _known[item.name] = item;
                    _items.Add(Read(item));
                    item = item.NextLevelItem;
                }
            }

            /// <summary>アイテム効果をシミュレーション用の定義へ変換する</summary>
            private SimItemDef Read(Item item)
            {
                var def = new SimItemDef
                {
                    name = item.name,
                    next = item.NextLevelItem != null ? item.NextLevelItem.name : string.Empty,
                };

                switch (item)
                {
                    case HealthUpItem:
                        def.kind = SimItemKind.HealthUp;
                        def.amount = SerializedReader.Int(item, "HealthIncreaseAmount");
                        break;
                    case Expcatcher:
                    {
                        def.kind = SimItemKind.ExpCatcher;
                        def.amount = SerializedReader.Float(item, "increaseamount");
                        def.rangeKey = PrefabReader.PickupRangeKey;
                        break;
                    }
                    case DamageUpItem:
                        def.kind = SimItemKind.DamageUp;
                        def.amount = SerializedReader.Float(item, "DamageIncreaseAmount");
                        break;
                    case KnockbackUpItem:
                        def.kind = SimItemKind.KnockbackUp;
                        def.amount = SerializedReader.Float(item, "KnockbackIncreaseAmount");
                        break;
                    case SyncWeaponItem:
                        def.kind = SimItemKind.SyncWeapon;
                        break;
                    default:
                        _warnings.Add($"アイテム {item.name} の種類 {item.GetType().Name} は未対応です。効果なしとして扱います");
                        def.kind = SimItemKind.Unknown;
                        break;
                }
                return def;
            }
        }
    }
}
