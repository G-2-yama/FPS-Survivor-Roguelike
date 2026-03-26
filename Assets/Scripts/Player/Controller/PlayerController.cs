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
    [SerializeField] private PlayerModel model;
    public PlayerModel Model => model;

    /// <summary>
    /// プレイヤーの移動状態を管理するステートマシン
    /// </summary>
    private StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;

    /// <summary>
    /// 移動入力値を保持
    /// </summary>
    private Vector2 moveInput;

    /// <summary>
    /// 視点入力値を保持
    /// </summary>
    private Vector2 lookInput;

    /// <summary>
    /// 走行入力状態を保持
    /// </summary>
    private bool isRunning;

    /// <summary>
    /// カメラの現在ピッチ角を保持
    /// </summary>
    private float pitch;

    /// <summary>
    /// ステートマシンを初期化し、現在のピッチ角を設定
    /// </summary>
    void Awake()
    {
        stateMachine = new StateMachine();

        if (cameraPitchTransform != null)
        {
            float x = cameraPitchTransform.localEulerAngles.x;
            pitch = x > 180f ? x - 360f : x;
        }
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
        model.moveInput = moveInput;
        model.lookInput = lookInput;
        model.isRunning = isRunning;

        ApplyLook();

        stateMachine.Update();
    }

    /// <summary>
    /// 視点入力を適用してプレイヤーのヨー回転とカメラのピッチ回転を更新
    /// </summary>
    private void ApplyLook()
    {
        float yawDelta = model.lookInput.x * model.LookSensitivity;
        float pitchDelta = model.lookInput.y * model.LookSensitivity;

        model.transform.Rotate(Vector3.up, yawDelta, Space.World);

        pitch = Mathf.Clamp(pitch - pitchDelta, model.MinPitch, model.MaxPitch);
        Vector3 localEuler = cameraPitchTransform.localEulerAngles;
        localEuler.x = pitch;
        cameraPitchTransform.localEulerAngles = localEuler;
    }

    /// <summary>
    /// 移動入力を受け取り、移動状態を更新
    /// </summary>
    /// <param name="context">Input System のコールバックコンテキスト</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// 走行入力を受け取り、走行状態を更新
    /// </summary>
    /// <param name="context">Input System のコールバックコンテキスト</param>
    public void OnRun(InputAction.CallbackContext context)
    {
        isRunning = context.ReadValueAsButton();
    }

    /// <summary>
    /// 視点入力を受け取り、マウスによる視点移動に利用
    /// </summary>
    /// <param name="context">Input System のコールバックコンテキスト</param>
    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

}
