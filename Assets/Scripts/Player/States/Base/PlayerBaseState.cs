/// <summary>
/// プレイヤーの基底ステートの基底クラス。
/// 共通の入力判定と遷移ヘルパーを提供する。
/// </summary>
public abstract class PlayerBaseState : IState
{
    /// <summary>
    /// この基底ステートが非接地状態かどうか
    /// </summary>
    public virtual bool IsUngrounded => false;

    /// <summary>
    /// この基底ステートが死亡状態かどうか
    /// </summary>
    public virtual bool IsDead => false;

    /// <summary>
    /// 基底ステートが参照するプレイヤー制御コンテキスト
    /// </summary>
    protected PlayerContext context;

    /// <summary>
    /// 基底ステートの遷移を行うステートマシン
    /// </summary>
    protected PlayerBaseStateMachine baseStateMachine;

    /// <summary>
    /// 動作ステートへの遷移を行うステートマシン
    /// </summary>
    protected PlayerActionStateMachine actionStateMachine;

    /// <summary>
    /// 移動入力がないとみなす最小入力値の二乗
    /// </summary>
    private const float MoveInputDeadzoneSqr = 0.0001f;

    /// <summary>
    /// 基底ステートを初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="baseStateMachine">基底ステートマシン。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerBaseState(
        PlayerContext context,
        PlayerBaseStateMachine baseStateMachine,
        PlayerActionStateMachine actionStateMachine)
    {
        this.context = context;
        this.baseStateMachine = baseStateMachine;
        this.actionStateMachine = actionStateMachine;
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
        return context.Input.MoveInput.sqrMagnitude > MoveInputDeadzoneSqr;
    }

    /// <summary>
    /// 非接地なら非接地ステートへ遷移する
    /// </summary>
    /// <returns>遷移した場合はtrue。</returns>
    protected bool TryTransitionToUngroundedState()
    {
        if (!context.Motor.IsGrounded())
        {
            baseStateMachine.ChangeUngroundedState();
            return true;
        }

        return false;
    }

    /// <summary>
    /// ダッシュ要求を消費し、開始可能であればダッシュ動作へ遷移する
    /// </summary>
    /// <returns>ダッシュ動作へ遷移した場合はtrue</returns>
    protected bool TryTransitionByDashRequest()
    {
        if (!context.Input.ConsumeDashRequest())
        {
            return false;
        }

        return actionStateMachine.TryChangeDashState();
    }

    /// <summary>
    /// ジャンプ要求があればジャンプを実行し非接地ステートへ遷移する
    /// </summary>
    /// <returns>遷移した場合はtrue。</returns>
    protected bool TryTransitionByJumpRequest()
    {
        if (!context.Input.ConsumeJumpRequest())
        {
            return false;
        }

        if (!context.TryJump())
        {
            return false;
        }

        baseStateMachine.ChangeUngroundedState();
        return true;
    }

    /// <summary>
    /// 地上時の入力状態に応じてIdle/Walkingへ遷移する
    /// </summary>
    protected void TransitionToGroundStatusByInput()
    {
        if (!HasMoveInput())
        {
            baseStateMachine.ChangeIdleState();
            return;
        }

        baseStateMachine.ChangeWalkingState();
    }
}
