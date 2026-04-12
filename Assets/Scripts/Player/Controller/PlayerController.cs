using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤー入力を受け取り、モデル反映と状態更新を行うコントローラー
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private Player player;
    public Player Player => player;

    [SerializeField] private Rigidbody playerRigidbody;

    /// <summary>
    /// 左武器の入力・攻撃処理を管理するコントローラー
    /// </summary>
    [SerializeField] private WeaponController leftWeaponController;
    public WeaponController LeftWeaponController => leftWeaponController;

    /// <summary>
    /// 右武器の入力・攻撃処理を管理するコントローラー
    /// </summary>
    [SerializeField] private WeaponController rightWeaponController;
    public WeaponController RightWeaponController => rightWeaponController;

    /// <summary>
    /// カメラのピッチ回転を制御するためのTransform
    /// </summary>
    [SerializeField] private Transform cameraPitchTransform;

    private StateMachine<PlayerState> stateMachine;
    public StateMachine<PlayerState> StateMachine => stateMachine;

    /// <summary>
    /// 移動処理
    /// </summary>
    private PlayerMover mover;
    public PlayerMover Mover => mover;

    /// <summary>
    /// 視点処理
    /// </summary>
    private PlayerLook look;
    public PlayerLook Look => look;

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
    /// ジャンプ入力要求を保持
    /// </summary>
    private bool jumpRequested;

    /// <summary>
    /// 初期化
    /// </summary>
    void Awake()
    {
        playerRigidbody.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        stateMachine = new StateMachine<PlayerState>();

        mover = new PlayerMover(transform, playerRigidbody, player.Config);
        look = new PlayerLook(transform, cameraPitchTransform, player.Config);
    }

    /// <summary>
    /// 初期状態として待機状態に遷移
    /// </summary>
    void Start()
    {
        player.OnDeath += OnPlayerDeath;
        stateMachine.ChangeState(new AliveState(this));
    }

    /// <summary>
    /// 入力値をモデルへ反映し、視点処理と状態更新を実行
    /// </summary>
    void Update()
    {
        isGrounded = mover.IsGrounded();

        stateMachine.Update();
    }

    void OnDestroy()
    {
        player.OnDeath -= OnPlayerDeath;
    }

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    public void Jump()
    {
        mover.Jump();
    }

    /// <summary>
    /// ジャンプ入力要求を1回だけ取り出す
    /// </summary>
    /// <returns>ジャンプ入力要求があった場合はtrue</returns>
    public bool ConsumeJumpRequest()
    {
        if (!jumpRequested)
        {
            return false;
        }

        jumpRequested = false;
        return true;
    }

    /// <summary>
    /// プレイヤーを死亡状態にさせる
    /// </summary>
    public void OnPlayerDeath()
    {
        Debug.Log("Playerが死亡しました");

        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
        isSprinting = false;
        jumpRequested = false;
        playerRigidbody.linearVelocity = Vector3.zero;

        stateMachine.ChangeState(new DeathState(this));
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

    /// <summary>
    /// ジャンプ入力を受け取る
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            jumpRequested = true;
        }
    }

    /// <summary>
    /// 射撃入力を左武器コントローラーへ中継
    /// </summary>
    public void OnLeftFire(InputAction.CallbackContext context)
    {
        if (player.LeftWeapon.WeaponData == null)
        {
            return;
        }

        if(stateMachine.CurrentState is AliveState)
        {
            leftWeaponController?.OnFire(context);
        }
    }

    /// <summary>
    /// 射撃入力を右武器コントローラーへ中継
    /// </summary>
    public void OnRightFire(InputAction.CallbackContext context)
    {
        if (player.RightWeapon.WeaponData == null)
        {
            return;
        }

        if(stateMachine.CurrentState is AliveState)
        {
            rightWeaponController?.OnFire(context);
        }
    }

    /// <summary>
    /// リロード入力を左武器コントローラーへ中継
    /// </summary>
    public void OnLeftReload(InputAction.CallbackContext context)
    {
        if (player.LeftWeapon.WeaponData == null)
        {
            return;
        }

        if(stateMachine.CurrentState is AliveState)
        {
            leftWeaponController?.OnReload(context);
        }
    }

    /// <summary>
    /// リロード入力を右武器コントローラーへ中継
    /// </summary>
    public void OnRightReload(InputAction.CallbackContext context)
    {
        if (player.RightWeapon.WeaponData == null)
        {
            return;
        }
        if(stateMachine.CurrentState is AliveState)
        {
            rightWeaponController?.OnReload(context);
        }
    }
}
