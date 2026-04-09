/// <summary>
/// プレイヤー移動サブステートの基底クラス。
/// 共通の入力判定と遷移ヘルパーを提供する。
/// </summary>
public abstract class PlayerMoveState : IState
{
    protected PlayerController controller;
    protected PlayerMoveStateMachine moveStateMachine;


    private const float MoveInputDeadzoneSqr = 0.0001f;

    /// <summary>
    /// 基底移動ステートを初期化する。
    /// </summary>
    /// <param name="controller">プレイヤー制御本体。</param>
    /// <param name="moveStateMachine">移動サブステートマシン。</param>
    public PlayerMoveState(PlayerController controller,
                           PlayerMoveStateMachine moveStateMachine)
    {
        this.controller = controller;
        this.moveStateMachine = moveStateMachine;
    }

    /// <summary>
    /// 状態開始時の初期化処理
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// 状態のフレーム更新処理
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// 状態終了時の後処理
    /// </summary>
    public virtual void Exit() { }

    /// <summary>
    /// 移動入力がデッドゾーンを超えているか判定する。
    /// </summary>
    /// <returns>移動入力が有効な場合はtrue。</returns>
    protected bool HasMoveInput()
    {
        return controller.MoveInput.sqrMagnitude > MoveInputDeadzoneSqr;
    }

    /// <summary>
    /// 非接地なら空中ステートへ遷移する
    /// </summary>
    /// <returns>遷移した場合はtrue。</returns>
    protected bool TryTransitionToAirState()
    {
        if (!controller.IsGrounded)
        {
            moveStateMachine.ChangeAirState();
            return true;
        }

        return false;
    }

    /// <summary>
    /// ジャンプ要求があればジャンプを実行し空中ステートへ遷移する
    /// </summary>
    /// <returns>遷移した場合はtrue。</returns>
    protected bool TryTransitionByJumpRequest()
    {
        if (!controller.ConsumeJumpRequest())
        {
            return false;
        }

        if (!controller.TryJump())
        {
            return false;
        }

        moveStateMachine.ChangeAirState();
        return true;
    }

    /// <summary>
    /// 地上時の入力状態に応じてIdle/Walk/Sprintへ遷移する
    /// </summary>
    protected void TransitionToGroundMoveStateByInput()
    {
        if (!HasMoveInput())
        {
            moveStateMachine.ChangeIdleState();
            return;
        }

        if (controller.IsSprinting)
        {
            moveStateMachine.ChangeSprintState();
            return;
        }

        moveStateMachine.ChangeWalkState();
    }
}
