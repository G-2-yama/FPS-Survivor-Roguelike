using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーに必要な依存参照を組み立て、入力受付と状態更新を中継するコントローラー
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    /// <summary>
    /// 体力と移動設定を保持するプレイヤーモデル
    /// </summary>
    [SerializeField] private PlayerHealth player;

    /// <summary>
    /// 物理挙動を無効化してCharacterController移動へ寄せるためのRigidbody
    /// </summary>
    [SerializeField] private Rigidbody playerRigidbody;

    /// <summary>
    /// プレイヤー移動に使用するCharacterController
    /// </summary>
    private CharacterController playerCharacterController;

    /// <summary>
    /// 武器の入力・攻撃処理を管理するコントローラー
    /// </summary>
    [SerializeField] private WeaponController weaponController;

    /// <summary>
    /// カメラのピッチ回転を制御するためのTransform
    /// </summary>
    [SerializeField] private Transform cameraPitchTransform;

    /// <summary>
    /// 状態属性ステートと動作ステートをまとめて更新する管理クラス
    /// </summary>
    private PlayerStateCoordinator stateController;

    /// <summary>
    /// プレイヤー制御に必要な参照をまとめたコンテキスト
    /// </summary>
    private PlayerContext context;

    /// <summary>
    /// CharacterControllerを使った移動計算
    /// </summary>
    private PlayerMotor motor;

    /// <summary>
    /// プレイヤー本体とカメラピッチの視点制御
    /// </summary>
    private PlayerLookController look;

    /// <summary>
    /// 入力状態を保持するモデル
    /// </summary>
    private PlayerInputState inputState;

    /// <summary>
    /// Input Systemのコールバックを処理するハンドラー
    /// </summary>
    private PlayerInputHandler inputHandler;

    /// <summary>
    /// 必要なコンポーネントとプレイヤー制御クラスを初期化する
    /// </summary>
    void Awake()
    {
        playerCharacterController = GetComponent<CharacterController>();

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.isKinematic = true;
            playerRigidbody.useGravity = false;
        }

        motor = new PlayerMotor(transform, playerCharacterController, player.Config);
        look = new PlayerLookController(transform, cameraPitchTransform, player.Config);
        inputState = new PlayerInputState();
        context = new PlayerContext(player, weaponController, motor, look, inputState);
        stateController = new PlayerStateCoordinator(context);
        inputHandler = new PlayerInputHandler(inputState, weaponController, () => !stateController.IsDead);
    }

    /// <summary>
    /// 死亡通知を購読する
    /// </summary>
    void Start()
    {
        player.OnDeath += OnPlayerDeath;
    }

    /// <summary>
    /// 接地状態を更新し、プレイヤー状態を1フレーム分進める
    /// </summary>
    void Update()
    {
        motor.RefreshGroundState();

        stateController.Update();
    }

    /// <summary>
    /// 死亡通知の購読を解除する
    /// </summary>
    void OnDestroy()
    {
        player.OnDeath -= OnPlayerDeath;
    }

    /// <summary>
    /// CharacterControllerが壁に当たったときに水平速度を壁面へ沿わせる
    /// </summary>
    /// <param name="hit">CharacterControllerの接触情報</param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        motor.ResolveWallHit(hit.normal);
    }

    /// <summary>
    /// プレイヤーを死亡状態にさせる
    /// </summary>
    public void OnPlayerDeath()
    {
        Debug.Log("Playerが死亡しました");

        inputState.Reset();
        motor.Stop();

        stateController.ChangeDeadStatusState();
    }

    /// <summary>
    /// 移動入力を受け取り、移動状態を更新
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        inputHandler.OnMove(context);
    }

    /// <summary>
    /// ダッシュ入力を受け取り、ダッシュ要求を更新
    /// </summary>
    public void OnSprint(InputAction.CallbackContext context)
    {
        inputHandler.OnSprint(context);
    }

    /// <summary>
    /// 視点入力を受け取り、マウスによる視点移動に利用
    /// </summary>
    public void OnLook(InputAction.CallbackContext context)
    {
        inputHandler.OnLook(context);
    }

    /// <summary>
    /// ジャンプ入力を受け取る
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        inputHandler.OnJump(context);
    }

    /// <summary>
    /// 射撃入力を武器コントローラーへ中継
    /// </summary>
    public void OnFire(InputAction.CallbackContext context)
    {
        inputHandler.OnFire(context);
    }

    /// <summary>
    /// リロード入力を武器コントローラーへ中継
    /// </summary>
    public void OnReload(InputAction.CallbackContext context)
    {
        inputHandler.OnReload(context);
    }
}
