using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤー入力を受け取り、モデル反映と状態更新を行うコントローラー
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// カメラのピッチ回転を制御するためのTransform
    /// </summary>
    [SerializeField] private Transform cameraPitchTransform;

    /// <summary>
    /// プレイヤーの各種パラメータを保持するモデル
    /// </summary>
    [SerializeField] private PlayerConfig config;
    public PlayerConfig Model => config;

    /// <summary>
    /// プレイヤーの移動状態を管理するステートマシン
    /// </summary>
    private StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;

    /// <summary>
    /// 移動処理
    /// </summary>
    private PlayerMover mover;

    /// <summary>
    /// 視点処理
    /// </summary>
    private PlayerLook look;

    /// <summary>
    /// 移動入力値を保持
    /// </summary>
    private Vector2 moveInput;
    public Vector2 MoveInput => moveInput;

    /// <summary>
    /// 視点入力値を保持
    /// </summary>
    private Vector2 lookInput;
    public Vector2 LookInput => lookInput;

    /// <summary>
    /// スプリント入力状態を保持
    /// </summary>
    private bool isSprinting;
    public bool IsSprinting => isSprinting;

    /// <summary>
    /// 接地状態を保持
    /// </summary>
    private bool isGrounded;
    public bool IsGrounded => isGrounded;

    /// <summary>
    /// 初期化
    /// </summary>
    void Awake()
    {
        stateMachine = new StateMachine();

        mover = new PlayerMover(transform, config);
        look = new PlayerLook(transform, cameraPitchTransform, config);
    }

    /// <summary>
    /// 初期状態として待機状態に遷移
    /// </summary>
    void Start()
    {
        stateMachine.ChangeState(new PlayerIdleState(this));
    }

    /// <summary>
    /// 入力値をモデルへ反映し、視点処理と状態更新を実行
    /// </summary>
    void Update()
    {
        look.ApplyLook(lookInput);

        isGrounded = mover.IsGrounded();

        stateMachine.Update();
    }

    /// <summary>
    /// 移動処理
    /// </summary>
    public void Move()
    {
        mover.Move(moveInput, isSprinting);
    }

    /// <summary>
    /// 移動入力を受け取り、移動状態を更新
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// スプリント入力を受け取り、スプリント状態を更新
    /// </summary>
    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    /// <summary>
    /// 視点入力を受け取り、マウスによる視点移動に利用
    /// </summary>
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }
}
