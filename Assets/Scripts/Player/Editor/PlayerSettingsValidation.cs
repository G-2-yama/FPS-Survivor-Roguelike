using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>設定移行とプレイヤー状態および物理接触の統合検証を提供する</summary>
[InitializeOnLoad]
public static class PlayerSettingsValidation
{
    private const string BatchKey = "PlayerSettingsValidation.Batch";
    private const string ChecksKey = "PlayerSettingsValidation.Checks";
    private static readonly List<string> checks = new();
    private static Player first;
    private static Player second;
    private static PlayerContactTarget target;
    private static int contactStartHP;
    private static int phase;
    private static double nextCheck;
    private static double deadline;
    private static string originalStatsJson;

    /// <summary>バッチ検証時だけプレイ開始後の検証を再開する</summary>
    static PlayerSettingsValidation()
    {
        EditorApplication.playModeStateChanged += HandlePlayState;
    }

    /// <summary>既存の5分類SOをシーン編集なしで検証する</summary>
    [MenuItem("Tools/Player/設定SOを検証")]
    public static void ValidateSettings()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:PlayerSettingsAsset"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PlayerSettingsAsset asset = AssetDatabase.LoadAssetAtPath<PlayerSettingsAsset>(path);
            Assert(asset != null && asset.Validate(out _), path + "の設定が不正です");
            foreach (FieldInfo field in asset.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (field.GetCustomAttribute<SerializeField>() == null) continue;
                Assert(field.GetCustomAttribute<TooltipAttribute>() != null, path + "の説明がありません: " + field.Name);
                Assert(field.GetCustomAttribute<PlayerSettingLabelAttribute>() != null, path + "の表示名がありません: " + field.Name);
            }
        }
        Debug.Log("プレイヤー設定SOの検証に成功しました");
    }

    /// <summary>隔離した検証用プロジェクトで移行値とプレイ中の統合検証を開始する</summary>
    public static void RunBatch()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("バッチ検証は隔離プロジェクトで実行してください");
        try
        {
            checks.Clear();
            ValidateSettings();
            Record("5分類SOの数値検証、英語ラベル、Tooltip");
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Single);
            Player player = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                    Assert(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0,
                        "Missing Script: " + child.name);
                if (player == null) player = root.GetComponentInChildren<Player>(true);
            }
            Assert(player != null && player.Config.Validate(out _), "GameSceneのプレイヤー設定が不足しています");
            Assert(player.Config.Stats.InitialHP == 1000, "初期HPの移行値が異なります");
            Assert(Mathf.Approximately(player.Config.Stats.DashDistance, 5.5f), "ダッシュ距離の移行値が異なります");
            Assert(Mathf.Approximately(player.Config.Progression.LevelUpRequiredExp, 50f), "必要経験値の移行値が異なります");
            Assert(Mathf.Approximately(player.Config.Progression.LevelUpRate, 1.08f), "経験値倍率の移行値が異なります");
            Assert(player.GetComponent<ExpManager>() == null, "旧経験値コンポーネントが残っています");
            Assert(player.GetComponent<TimedBuffManager>() != null, "強化管理コンポーネントの移行に失敗しています");
            Record("GameSceneの移行値、設定参照、Missing Script");
            TestEnemyPushConfiguration();
            TestBalanceSimulator();
            TestProgression(player.Config.Progression);
            TestPushMath();
            SessionState.SetString(ChecksKey, string.Join("\n", checks));
            SessionState.SetBool(BatchKey, true);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception exception) { Finish(exception); }
    }

    /// <summary>敵の押し出し既定動作とアイテムボックスの例外設定を確認する</summary>
    private static void TestEnemyPushConfiguration()
    {
        EnemyConfig itemBoxConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>("Assets/Datas/spownobject/itembox.asset");
        GameObject itemBoxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/spownobject/アイテムボックス.prefab");
        Enemy itemBoxEnemy = itemBoxPrefab != null ? itemBoxPrefab.GetComponent<Enemy>() : null;
        var serializedEnemy = itemBoxEnemy != null ? new SerializedObject(itemBoxEnemy) : null;
        Assert(itemBoxPrefab != null && itemBoxPrefab.GetComponent<PlayerObstacle>() != null, "アイテムボックスPrefabにPlayerObstacleがありません");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("PlayerBody");
        int obstacleLayer = LayerMask.NameToLayer("PlayerObstacle");
        Assert(itemBoxPrefab != null && itemBoxPrefab.layer == obstacleLayer, "アイテムボックスPrefabに専用レイヤーがありません");
        Assert(Physics.GetIgnoreLayerCollision(enemyLayer, obstacleLayer), "敵とプレイヤー遮蔽物の物理衝突が無効になっていません");
        Assert(!Physics.GetIgnoreLayerCollision(playerLayer, obstacleLayer), "プレイヤーとアイテムボックスの物理衝突が無効になっています");
        Assert(serializedEnemy != null && serializedEnemy.FindProperty("config").objectReferenceValue == itemBoxConfig,
            "アイテムボックスPrefabが専用の敵設定を参照していません");
        Record("敵設定、プレイヤー遮蔽物レイヤー、アイテムボックスの例外設定");
    }

    /// <summary>既存のバランス検証ツールが新しいSOから開始値を読み取れることを確認する</summary>
    private static void TestBalanceSimulator()
    {
        var settings = AssetDatabase.LoadAssetAtPath<BalanceSim.Editor.BalanceSimSettings>(
            "Assets/Datas/BalanceSim/BalanceSimSettings.asset");
        Assert(settings != null, "既存のバランス検証設定がありません");
        BalanceSim.SimInput built = null;
        BalanceSim.Editor.SimInputBuilder.Build(settings,
            new[] { BalanceSim.Editor.SimValueCase.None }, (index, input) => built = input,
            new List<string>(), out _);
        Assert(built != null && built.player.maxHp == 1000 && Mathf.Approximately(built.player.runSpeed, 10f)
            && Mathf.Approximately(built.player.levelUpRequiredExp, 50f)
            && Mathf.Approximately(built.player.levelUpRate, 1.08f),
            "バランス検証用の初期能力または成長値の読取に失敗しました");
        Record("既存のバランスシミュレーターによる新SOの読取");
    }

    /// <summary>複数レベルアップと倍率1の境界を検証する</summary>
    private static void TestProgression(PlayerProgressionSettings settings)
    {
        var progression = new PlayerProgression(settings);
        Assert(progression.AddExperience(200f) == 3, "複数レベルアップの回数が異なります");
        Assert(Mathf.Abs(progression.Experience - 37.68f) < 0.001f, "残余経験値が異なります");
        Assert(Mathf.Abs(progression.RequiredExperience - 62.9856f) < 0.001f, "次の必要経験値が異なります");
        var constant = Object.Instantiate(settings);
        var serialized = new SerializedObject(constant);
        serialized.FindProperty("levelUpRate").floatValue = 1f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var constantProgression = new PlayerProgression(constant);
        Assert(constantProgression.AddExperience(200f) == 4 && constantProgression.Experience == 0f,
            "経験値倍率1の境界計算が不正です");
        Object.DestroyImmediate(constant);
        Record("複数レベルアップ、残余経験値、倍率1の境界");
    }

    /// <summary>中心一致時と垂直位置の保持を検証する</summary>
    private static void TestPushMath()
    {
        Vector3 position = new Vector3(0f, 3f, 0f);
        Vector3 corrected = PlayerPushSolver.CorrectPosition(position, position, 1f, Vector3.zero);
        Assert(Mathf.Approximately(corrected.y, position.y) && Mathf.Approximately(corrected.x, 1f),
            "中心一致時の水平押し出しが不正です");
        Record("中心一致時の方向補完と高さの保持");
    }

    /// <summary>プレイモードの実際の物理更新を使う検証を開始する</summary>
    private static void HandlePlayState(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(BatchKey, false)) return;
        try
        {
            checks.Clear();
            checks.AddRange(SessionState.GetString(ChecksKey, "").Split('\n'));
            first = CreatePlayer("検証プレイヤーA", new Vector3(0f, 5f, 0f));
            second = CreatePlayer("検証プレイヤーB", new Vector3(10f, 5f, 0f));
            TestPlayerObstacleCollision(second);
            originalStatsJson = JsonUtility.ToJson(first.Config.Stats);
            Assert(first.Settings.Stats != second.Settings.Stats && first.Settings.Stats != first.Config.Stats,
                "開始時の設定複製が共有されています");
            first.Stats.AddMaxHP(200);
            first.TakeDamage(17, 0f);
            Assert(first.Health.MaxHP == first.Stats.MaxHP && first.Health.CurrentHP == 1183,
                "最大HPと現在HPが一致しません");
            Assert(second.Health.MaxHP == 1000 && second.Health.CurrentHP == 1000, "別プレイヤーへ体力が伝播しています");
            first.AddExp(200);
            Assert(first.Level == 4 && first.PendingLevelUps == 3 && second.Level == 1, "成長状態が分離されていません");
            Record("共有SOからの状態分離、最大HP、成長状態、選択待ち回数");
            second.DisableExpGain();
            second.MakeInvincible();
            second.AddExp(200);
            second.TakeDamage(17, 0f);
            Assert(second.Level == 1 && second.PendingLevelUps == 0 && second.Health.CurrentHP == 1000,
                "経験値の停止か無敵が効いていません");
            Record("経験値の停止と無敵");
            TestMovement(second);
            contactStartHP = first.Health.CurrentHP;
            var enemy = new GameObject("検証接触対象");
            enemy.layer = 6;
            enemy.transform.position = first.transform.position + Vector3.right * 0.2f;
            Rigidbody body = enemy.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezeRotation;
            enemy.AddComponent<SphereCollider>().radius = 0.25f;
            var child = new GameObject("追加Collider");
            child.layer = 6;
            child.transform.SetParent(enemy.transform, false);
            child.AddComponent<SphereCollider>().radius = 0.25f;
            var damage = enemy.AddComponent<ToutchDamageController>();
            target = enemy.AddComponent<PlayerContactTarget>();
            target.BeginLife(body, damage);
            Physics.SyncTransforms();
            phase = 0;
            nextCheck = Time.fixedTimeAsDouble + 0.2d;
            deadline = EditorApplication.timeSinceStartup + 30d;
            EditorApplication.update += TickPhysicsChecks;
        }
        catch (Exception exception) { Finish(exception); }
    }

    /// <summary>実際のCharacterControllerが専用遮蔽物を通り抜けないことを確認する</summary>
    private static void TestPlayerObstacleCollision(Player player)
    {
        CharacterController controller = player.GetComponent<CharacterController>();
        Vector3 start = player.transform.position;
        var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = "検証用プレイヤー遮蔽物";
        obstacle.layer = LayerMask.NameToLayer("PlayerObstacle");
        obstacle.transform.position = start + Vector3.forward * 1.5f;
        Physics.SyncTransforms();

        controller.Move(Vector3.forward * 2f);
        float movedDistance = player.transform.position.z - start.z;
        Assert(movedDistance < 1f, "プレイヤーがPlayerObstacleレイヤーを通り抜けました");

        player.transform.position = start;
        // Destroyの反映を待たず次の移動検証から遮蔽物を除く
        obstacle.SetActive(false);
        Physics.SyncTransforms();
        Object.Destroy(obstacle);
        Record("実際のCharacterControllerによるプレイヤー遮蔽物の通過防止");
    }

    /// <summary>SO参照を割り当ててからプレイヤーのAwakeを実行する</summary>
    private static Player CreatePlayer(string name, Vector3 position)
    {
        var root = new GameObject(name);
        root.SetActive(false);
        root.transform.position = position;
        root.AddComponent<CharacterController>();
        root.AddComponent<CapsuleCollider>().isTrigger = true;
        Rigidbody body = root.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        var config = root.AddComponent<PlayerConfig>();
        var serialized = new SerializedObject(config);
        string[] fields = { "stats", "control", "collision", "cameraSettings", "progression" };
        string[] names = { "Stats", "Control", "Collision", "Camera", "Progression" };
        for (int i = 0; i < fields.Length; i++)
            serialized.FindProperty(fields[i]).objectReferenceValue = AssetDatabase.LoadAssetAtPath<PlayerSettingsAsset>(
                "Assets/Datas/Player/GameScene" + names[i] + "Settings.asset");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Player player = root.AddComponent<Player>();
        var playerReferences = new SerializedObject(player);
        playerReferences.FindProperty("config").objectReferenceValue = config;
        playerReferences.FindProperty("timedBuffManager").objectReferenceValue = root.AddComponent<TimedBuffManager>();
        playerReferences.ApplyModifiedPropertiesWithoutUndo();
        var collision = root.AddComponent<PlayerCollisionController>();
        var collisionReferences = new SerializedObject(collision);
        collisionReferences.FindProperty("config").objectReferenceValue = config;
        collisionReferences.FindProperty("controller").objectReferenceValue = root.GetComponent<CharacterController>();
        collisionReferences.FindProperty("bodyTrigger").objectReferenceValue = root.GetComponent<CapsuleCollider>();
        collisionReferences.ApplyModifiedPropertiesWithoutUndo();
        FixedComponentSetup.ConfigureHierarchy(root);
        root.SetActive(true);
        collision.Initialize(player, player.Settings.Collision);
        return player;
    }

    /// <summary>強化後のダッシュ距離と長押しジャンプを実際のモーターで確認する</summary>
    private static void TestMovement(Player player)
    {
        player.Stats.AddDashDistance(3f);
        player.Stats.AddDashDuration(0.08f);
        player.Stats.AddDashCooldown(0.2f);
        player.Stats.AddJumpForce(2f);
        var motor = new PlayerMotor(player.GetComponent<CharacterController>(), player.Settings.Collision);
        var jump = new PlayerJumpController(motor, player.Settings.Control, player.Stats);
        var locomotion = new PlayerLocomotion(player.transform, motor, jump, player.Settings.Control, player.Stats);
        Vector3 start = player.transform.position;
        locomotion.MoveDash(Vector3.forward, player.Stats.DashDuration, player.Stats.DashDistance / player.Stats.DashDuration);
        Assert(Mathf.Abs(player.transform.position.z - start.z - 8.5f) < 0.01f, "強化後のダッシュ距離が反映されていません");
        jump.Jump();
        jump.ApplyJumpHold(true, player.Settings.Control.JumpHoldDuration);
        Assert(Mathf.Abs(motor.VerticalVelocity - 12f) < 0.01f, "強化後の長押しジャンプが反映されていません");
        Record("強化後のダッシュ距離と時間、長押しジャンプ");
    }

    /// <summary>物理コールバックの結果を待ち、再侵入と姿勢変更を順に検証する</summary>
    private static void TickPhysicsChecks()
    {
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("物理検証が時間内に完了しませんでした");
            if (Time.fixedTimeAsDouble < nextCheck) return;
            if (phase == 0)
            {
                Assert(first.Health.CurrentHP == contactStartHP - 100,
                    $"複数Colliderの接触ダメージが不正です 現在HP={first.Health.CurrentHP} 期待HP={contactStartHP - 100} 固定時刻={Time.fixedTimeAsDouble}");
                Assert(Mathf.Abs(target.Body.position.y - 5f) < 0.01f, "押し出しによって敵の高さが変わりました");
                Assert((target.Body.position - first.transform.position).magnitude >= 0.74f, "敵が本体の内側に残っています");
                Record("実際のTrigger接触、複数Colliderの一度だけのダメージ、水平押し出し");
                target.gameObject.SetActive(false);
                target.transform.position = first.transform.position + Vector3.right * 0.2f;
                target.gameObject.SetActive(true);
                target.BeginLife(target.Body, target.GetComponent<ToutchDamageController>(), false);
                Physics.SyncTransforms();
            }
            else if (phase == 1)
            {
                Assert(first.Health.CurrentHP == contactStartHP - 200, "無効化と再取得で接触状態が持ち越されました");
                Assert(!target.CanBePushed, "活動期間再開後に押し出し無効設定が保持されていません");
                Assert(Vector3.Distance(target.Body.position, first.transform.position) < 0.3f,
                    "押し出し無効の対象が移動しました");
                Record("押し出し無効時の位置維持と接触ダメージ、活動期間の再開");
                var motor = new PlayerMotor(first.GetComponent<CharacterController>(), first.Settings.Collision);
                motor.SetControllerHeight(1f);
            }
            else
            {
                CapsuleCollider zone = first.GetComponentInChildren<PlayerEnemyPushZone>().GetComponent<CapsuleCollider>();
                CharacterController body = first.GetComponent<CharacterController>();
                Assert(Mathf.Approximately(zone.height, body.height) && zone.center == body.center, "姿勢変更に接触領域が追従しません");
                CapsuleCollider damageTrigger = first.GetComponent<CapsuleCollider>();
                Assert(Mathf.Approximately(damageTrigger.height, body.height) && damageTrigger.center == body.center,
                    "姿勢変更に被弾用カプセルが追従しません");
                TestCrowd(body);
                TestAttackFilter(zone);
                Assert(JsonUtility.ToJson(first.Config.Stats) == originalStatsJson, "プレイ中に共有ステータスSOが変更されました");
                Record("姿勢変更への追従、共有SOを変更しない実行");
                Finish(null);
                return;
            }
            phase++;
            nextCheck = Time.fixedTimeAsDouble + 0.2d;
        }
        catch (Exception exception) { Finish(exception); }
    }

    /// <summary>多数の敵に囲まれた状態で本体の水平移動が高さを変えないことを確認する</summary>
    private static void TestCrowd(CharacterController controller)
    {
        var bodies = new List<GameObject>();
        for (int i = 0; i < 24; i++)
        {
            float angle = i * Mathf.PI * 2f / 24f;
            var enemy = new GameObject("密集検証対象");
            enemy.layer = 6;
            enemy.transform.position = controller.transform.position
                + new Vector3(Mathf.Cos(angle) * 0.3f, -0.3f, Mathf.Sin(angle) * 0.3f);
            enemy.AddComponent<Rigidbody>().isKinematic = true;
            enemy.AddComponent<SphereCollider>().radius = 0.3f;
            bodies.Add(enemy);
        }
        Physics.SyncTransforms();
        float height = controller.transform.position.y;
        controller.Move(Vector3.forward * 0.05f);
        Assert(Mathf.Abs(controller.transform.position.y - height) < 0.001f,
            "密集した敵との衝突でプレイヤーが持ち上がりました");
        foreach (GameObject body in bodies) Object.Destroy(body);
        Record("24体の敵に囲まれた水平移動で本体の高さを維持");
    }

    /// <summary>実体弾と射線検索の両方で補助Colliderを通過することを確認する</summary>
    private static void TestAttackFilter(Collider zone)
    {
        Assert(AttackCollisionFilter.ShouldIgnore(zone), "補助Colliderが攻撃除外対象に登録されていません");
        var projectileObject = new GameObject("検証弾");
        projectileObject.transform.position = Vector3.one * 100f;
        projectileObject.AddComponent<Rigidbody>().isKinematic = true;
        NormalProjectile projectile = projectileObject.AddComponent<NormalProjectile>();
        projectile.Initialize(1, 0f, 10f);
        projectile.SendMessage("OnTriggerEnter", zone);
        bool hit = (bool)typeof(ProjectileObject).GetField("hasHit", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(projectile);
        Assert(!hit && projectileObject.activeSelf, "補助Colliderで実体弾が命中処理されました");
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = new Vector3(2f, 5f, 0f);
        Physics.SyncTransforms();
        RaycastHit[] hits = Physics.SphereCastAll(new Ray(new Vector3(-3f, 4.5f, 0f), Vector3.right), 0.1f, 6f,
            ~0, QueryTriggerInteraction.Collide);
        bool foundZone = false;
        bool foundWall = false;
        foreach (RaycastHit candidate in hits)
        {
            if (AttackCollisionFilter.ShouldIgnore(candidate.collider)) foundZone = true;
            else if (candidate.collider == wall.GetComponent<Collider>()) foundWall = true;
        }
        Assert(foundZone && foundWall, "補助Colliderの先にある壁の射線判定が失われました");
        Object.Destroy(projectileObject);
        Object.Destroy(wall);
        Record("実体弾の通過、SphereCastの補助Collider除外と壁の保持");
    }

    /// <summary>検証条件を満たさない場合に理由付きで停止する</summary>
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    /// <summary>成功した検証を記録する</summary>
    private static void Record(string message)
    {
        checks.Add(message);
        Debug.Log("検証成功: " + message);
    }

    /// <summary>結果を保存し、バッチプロセスを終了する</summary>
    private static void Finish(Exception exception)
    {
        EditorApplication.update -= TickPhysicsChecks;
        SessionState.SetBool(BatchKey, false);
        File.WriteAllText("PlayerSettingsValidationResults.txt",
            string.Join("\n", checks) + "\n" + (exception == null ? "PASS" : exception.ToString()));
        if (exception != null) Debug.LogException(exception);
        EditorApplication.Exit(exception == null ? 0 : 1);
    }
}
