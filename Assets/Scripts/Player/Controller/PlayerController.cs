using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// Unity から届く入力やライフサイクルイベントをプレイヤー制御へ中継する。
/// ゲーム内データの更新は PlayerRuntime 側へ寄せ、MonoBehaviour は橋渡しに専念する。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// プレイヤーモデル
    /// </summary>
    [FormerlySerializedAs("playerHealth")]
    [SerializeField] private Player player;

    /// <summary>
    /// プレイヤー全体設定
    /// </summary>
    [SerializeField] private PlayerConfig playerConfig;

    /// <summary>
    /// 物理挙動を無効化してCharacterController移動へ寄せるためのRigidbody
    /// </summary>
    [SerializeField] private Rigidbody playerRigidbody;

    /// <summary>
    /// 武器の入力・攻撃処理を管理するコントローラー
    /// </summary>
    [SerializeField] private WeaponControllerManager weaponControllerManager;

    /// <summary>
    /// カメラの上下視点を制御するためのTransform
    /// </summary>
    [FormerlySerializedAs("cameraPitchTransform")]
    [SerializeField] private Transform cameraLookPivotTransform;

    /// <summary>
    /// プレイヤー移動に使用するCharacterController
    /// </summary>
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
        playerConfig = ResolvePlayerConfig();
        cameraLookPivotTransform = ResolveCameraLookPivotTransform();

        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        ConfigureRigidbodyForCharacterController();

        PlayerMotor motor = new PlayerMotor(playerCharacterController);
        PlayerJumpController jumpController = new PlayerJumpController(motor, playerConfig);
        PlayerLocomotion locomotion = new PlayerLocomotion(transform, motor, jumpController, playerConfig);
        PlayerLookController look = new PlayerLookController(transform, cameraLookPivotTransform, playerConfig);
        PlayerViewOffsetController viewOffset = new PlayerViewOffsetController(cameraLookPivotTransform, playerConfig);
        PlayerControlState controls = new PlayerControlState();
        PlayerCommandBuffer commands = new PlayerCommandBuffer();

        PlayerContext context = new PlayerContext(
            player,
            playerConfig,
            weaponControllerManager,
            motor,
            locomotion,
            jumpController,
            look,
            viewOffset,
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
        runtime?.HandleWallHit(hit.normal);
    }

    /// <summary>
    /// 死亡時の停止処理をランタイムへ委譲する
    /// </summary>
    private void OnPlayerDeath()
    {
        runtime?.HandleDeath();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        inputRelay?.OnMove(context);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        inputRelay?.OnSprint(context);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        inputRelay?.OnLook(context);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        inputRelay?.OnJump(context);
    }

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

        if (playerConfig == null)
        {
            Debug.LogError($"{nameof(PlayerController)} on {name} requires {nameof(playerConfig)}.", this);
            return false;
        }

        if (weaponControllerManager == null)
        {
            Debug.LogError($"{nameof(PlayerController)} on {name} requires {nameof(weaponControllerManager)}.", this);
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
    /// プレイヤー設定を解決する
    /// </summary>
    private PlayerConfig ResolvePlayerConfig()
    {
        if (playerConfig != null)
        {
            return playerConfig;
        }

        return GetComponent<PlayerConfig>();
    }
}
