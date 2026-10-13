using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// Unityから届く入力やライフサイクルイベントをプレイヤー制御へ中継する
/// ゲーム内データの更新はPlayerRuntime側へ寄せ、MonoBehaviourは橋渡しに専念する
/// </summary>
[RequireComponent(typeof(CharacterController), typeof(PlayerCollisionController))]
public class PlayerController : MonoBehaviour
{

    /// <summary>
    /// プレイヤーモデル
    /// </summary>
    [FormerlySerializedAs("playerHealth")]
    [SerializeField, PlayerSettingLabel("Player"), Tooltip("実行中の体力、成長、装備を保持するプレイヤー")] private Player player;

    /// <summary>
    /// プレイヤー全体設定
    /// </summary>
    private PlayerSettingsSnapshot settings;

    /// <summary>
    /// 物理挙動を無効化してCharacterController移動へ寄せるためのRigidbody
    /// </summary>
    [SerializeField, PlayerSettingLabel("Player Rigidbody"), Tooltip("本体にRigidbodyがある場合に指定する 開始時にKinematicへ設定する")] private Rigidbody playerRigidbody;

    /// <summary>
    /// 武器の入力・攻撃処理を管理するコントローラー
    /// </summary>
    [SerializeField, PlayerSettingLabel("Weapon Controller Manager"), Tooltip("入力と攻撃を管理する必須コンポーネント")] private WeaponControllerManager weaponControllerManager;

    /// <summary>
    /// カメラの上下視点を制御するためのTransform
    /// </summary>
    [FormerlySerializedAs("cameraPitchTransform")]
    [SerializeField, PlayerSettingLabel("Camera Look Pivot Transform"), Tooltip("上下視点とスライド時の高さ補正を適用するTransform")] private Transform cameraLookPivotTransform;

    [SerializeField, PlayerSettingLabel("Player Camera"), Tooltip("視野角演出を適用するカメラ 未指定なら子から取得する")] private Camera playerCamera;

    [Tooltip("ダッシュ・スライド・急降下時の軌跡 未設定の場合は軌跡を表示しません")]
    [SerializeField] private PlayerActionTrail actionTrail;

    /// <summary>
    /// ダッシュやスライディングなどのアクション中に集中線を表示するコンポーネント
    /// </summary>
    [SerializeField, PlayerSettingLabel("Action Concentration Line"), Tooltip("ダッシュやスライドなどに連動する任意の演出")] private PlayerActionConcentrationLine actionConcentrationLine;

    /// <summary>
    /// プレイヤーの衝突設定を適用するコンポーネント
    /// </summary>
    [SerializeField, PlayerSettingLabel("Player Collision Controller"), Tooltip("プレイヤーの衝突設定を適用するコンポーネント")]
    private PlayerCollisionController collisionController;
    private CharacterController playerCharacterController;

    /// <summary>
    /// 入力コールバックの反映先
    /// </summary>
    private PlayerInputRelay inputRelay;

    /// <summary>
    /// プレイヤー制御の実処理
    /// </summary>
    private PlayerRuntime runtime;

    /// <summary>
    /// 必要な参照とランタイムを初期化する
    /// </summary>
    private void Awake()
    {

        playerCharacterController = GetComponent<CharacterController>();
        player = ResolvePlayer();
        cameraLookPivotTransform = ResolveCameraLookPivotTransform();
        playerCamera = ResolvePlayerCamera();
        actionTrail = ResolveActionTrail();

        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }
        player.InitializeIfNeeded();
        if (player.Stats == null)
        {
            enabled = false;
            return;
        }
        settings = player.Settings;
        PlayerStats stats = player.Stats;
        collisionController.Initialize(player, settings.Collision);

        ConfigureRigidbodyForCharacterController();
        BoostFireCullerRunner.SetCamera(playerCamera);

        PlayerMotor motor = new PlayerMotor(playerCharacterController, settings.Collision);
        PlayerJumpController jumpController = new PlayerJumpController(motor, settings.Control, stats);
        PlayerLocomotion locomotion = new PlayerLocomotion(transform, motor, jumpController, settings.Control, stats);
        PlayerLookController look = new PlayerLookController(transform, cameraLookPivotTransform, settings.Control);
        PlayerViewOffsetController viewOffset = new PlayerViewOffsetController(cameraLookPivotTransform, settings.Camera);
        PlayerFovController fov = new PlayerFovController(playerCamera, settings.Camera);
        PlayerControlState controls = new PlayerControlState();
        PlayerCommandBuffer commands = new PlayerCommandBuffer();

        PlayerContext context = new PlayerContext(
            player,
            settings.Control,
            weaponControllerManager,
            motor,
            locomotion,
            jumpController,
            look,
            viewOffset,
            fov,
            actionTrail,
            actionConcentrationLine,
            controls,
            commands);

        runtime = new PlayerRuntime(context);
        inputRelay = new PlayerInputRelay(controls, commands, weaponControllerManager, () => !runtime.IsDead);
    }

    /// <summary>
    /// 死亡通知を購読する
    /// </summary>
    private void Start()
    {
        if (player == null || runtime == null)
        {
            return;
        }

        player.OnDeath += OnPlayerDeath;
    }

    /// <summary>
    /// 1フレーム分の更新をランタイムへ委譲する
    /// </summary>
    private void Update()
    {
        if (runtime == null)
        {
            return;
        }

        runtime.Update(Time.deltaTime);
    }

    /// <summary>
    /// 死亡通知の購読を解除する
    /// </summary>
    private void OnDestroy()
    {
        BoostFireCullerRunner.ClearCamera(playerCamera);
        if (player == null)
        {
            return;
        }

        player.OnDeath -= OnPlayerDeath;
    }

    /// <summary>
    /// CharacterControllerが壁に当たったときに水平速度を壁面へ沿わせる
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (settings != null && hit.normal.y > settings.Collision.RecoverableGroundNormalY
            && (settings.Collision.EnemyLayers.value & (1 << hit.collider.gameObject.layer)) == 0)
        {
            runtime?.HandleRecoverableGroundContact();
        }

        runtime?.HandleWallHit(hit.normal);
    }

    /// <summary>
    /// 死亡時の停止処理をランタイムへ委譲する
    /// </summary>
    private void OnPlayerDeath()
    {
        runtime?.HandleDeath();
    }

    /// <summary>移動入力をプレイヤー入力リレーへ渡す</summary>
    /// <param name="context">Input Systemの移動入力コンテキスト</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        inputRelay?.OnMove(context);
    }

    /// <summary>ダッシュ入力をプレイヤー入力リレーへ渡す</summary>
    /// <param name="context">Input Systemのダッシュ入力コンテキスト</param>
    public void OnSprint(InputAction.CallbackContext context)
    {
        inputRelay?.OnSprint(context);
    }

    /// <summary>視点入力をプレイヤー入力リレーへ渡す</summary>
    /// <param name="context">Input Systemの視点入力コンテキスト</param>
    public void OnLook(InputAction.CallbackContext context)
    {
        inputRelay?.OnLook(context);
    }

    /// <summary>ジャンプ入力をプレイヤー入力リレーへ渡す</summary>
    /// <param name="context">Input Systemのジャンプ入力コンテキスト</param>
    public void OnJump(InputAction.CallbackContext context)
    {
        inputRelay?.OnJump(context);
    }

    /// <summary>しゃがみ入力をプレイヤー入力リレーへ渡す</summary>
    /// <param name="context">Input Systemのしゃがみ入力コンテキスト</param>
    public void OnCrouch(InputAction.CallbackContext context)
    {
        inputRelay?.OnCrouch(context);
    }


    /// <summary>
    /// 必須参照が揃っているか確認する
    /// </summary>
    private bool ValidateReferences()
    {
        if (player == null)
        {
            Debug.LogError($"{nameof(PlayerController)} on {name} requires {nameof(player)}.", this);
            return false;
        }


        if (weaponControllerManager == null)
        {
            Debug.LogError($"{nameof(PlayerController)} on {name} requires {nameof(weaponControllerManager)}.", this);
            return false;
        }

        if (collisionController == null)
        {
            Debug.LogError($"{nameof(PlayerController)} on {name} requires {nameof(collisionController)}.", this);
            return false;
        }

        if (cameraLookPivotTransform == null)
        {
            Debug.LogError($"{nameof(PlayerController)} on {name} requires {nameof(cameraLookPivotTransform)}.", this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// CharacterController を使う前提の Rigidbody 設定へ寄せる
    /// </summary>
    private void ConfigureRigidbodyForCharacterController()
    {
        if (playerRigidbody == null)
        {
            return;
        }

        playerRigidbody.linearVelocity = Vector3.zero;
        playerRigidbody.isKinematic = true;
        playerRigidbody.useGravity = false;
    }

    /// <summary>
    /// 視点の上下回転を適用するTransformを解決する
    /// </summary>
    private Transform ResolveCameraLookPivotTransform()
    {
        if (cameraLookPivotTransform != null)
        {
            return cameraLookPivotTransform;
        }

        Camera childCamera = GetComponentInChildren<Camera>(true);
        if (childCamera != null)
        {
            Debug.LogWarning(
                $"{nameof(PlayerController)} on {name} auto-assigned {nameof(cameraLookPivotTransform)} from child camera.",
                this);
            return childCamera.transform;
        }

        return null;
    }

    /// <summary>設定済みのカメラか子オブジェクトのカメラを取得する</summary>
    private Camera ResolvePlayerCamera()
    {
        if (playerCamera != null)
        {
            return playerCamera;
        }

        return GetComponentInChildren<Camera>(true);
    }

    /// <summary>
    /// プレイヤーモデルを解決する
    /// </summary>
    private Player ResolvePlayer()
    {
        if (player != null)
        {
            return player;
        }

        return GetComponent<Player>();
    }

    /// <summary>
    /// 同じ GameObject に付与された軌跡コンポーネントを自動取得する
    /// </summary>
    private PlayerActionTrail ResolveActionTrail()
    {
        if (actionTrail != null)
        {
            return actionTrail;
        }

        return GetComponent<PlayerActionTrail>();
    }
}
