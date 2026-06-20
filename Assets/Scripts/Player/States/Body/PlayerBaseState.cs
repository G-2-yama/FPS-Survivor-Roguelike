/// <summary>
/// プレイヤーの継続的な身体状態を表す識別子
/// </summary>
public enum PlayerBodyStateId
{
    GroundedIdle,
    GroundedMove,
    Airborne,
    Dead,
}

/// <summary>
/// プレイヤーの継続的な身体状態を表す基底クラス。
/// Action が割り込み動作を表すのに対し、Body は常在する主状態を表す。
/// </summary>
public abstract class PlayerBodyState : IState
{
    /// <summary>
    /// この状態の識別子
    /// </summary>
    public abstract PlayerBodyStateId StateId { get; }

    /// <summary>
    /// プレイヤー制御コンテキスト
    /// </summary>
    protected PlayerContext context;

    /// <summary>
    /// 身体状態の遷移を扱うステートマシン
    /// </summary>
    protected PlayerBodyStateMachine bodyStateMachine;

    /// <summary>
    /// 一時アクション状態の遷移を扱うステートマシン
    /// </summary>
    protected PlayerActionStateMachine actionStateMachine;

    /// <summary>
    /// 停止扱いにする最小の移動入力量
    /// </summary>
    private const float MoveInputDeadzoneSqr = 0.0001f;

    /// <summary>
    /// 身体状態を初期化する
    /// </summary>
    protected PlayerBodyState(
        PlayerContext context,
        PlayerBodyStateMachine bodyStateMachine,
        PlayerActionStateMachine actionStateMachine)
    {
        this.context = context;
        this.bodyStateMachine = bodyStateMachine;
        this.actionStateMachine = actionStateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

    /// <summary>
    /// 移動入力がデッドゾーンを超えているか判定する
    /// </summary>
    protected bool HasMoveInput()
    {
        return context.Controls.MoveInput.sqrMagnitude > MoveInputDeadzoneSqr;
    }

    /// <summary>
    /// 空中へ移った場合に空中状態へ遷移する
    /// </summary>
    protected bool TryChangeToAirborneState()
    {
        if (!context.Motor.IsGrounded())
        {
            bodyStateMachine.ChangeToAirborneState();
            return true;
        }

        return false;
    }

    /// <summary>
    /// ダッシュ要求を消費し、条件を満たせばダッシュ状態へ遷移する
    /// </summary>
    protected bool TryChangeByDashCommand()
    {
        if (!context.Commands.TryConsumeDash())
        {
            return false;
        }

        return actionStateMachine.TryChangeToDashActionState();
    }

    protected bool TryChangeByCrouchActionToSlide()
    {
        if (!context.Commands.TryConsumeCrouchAction())
        {
            return false;
        }

        return actionStateMachine.TryChangeToSlideActionState();
    }

    protected bool TryChangeByCrouchActionToFastFall()
    {
        if (!context.Commands.TryConsumeCrouchAction())
        {
            return false;
        }

        return actionStateMachine.TryChangeToFastFallActionState();
    }

    /// <summary>
    /// ジャンプ要求を消費し、成功した場合は空中状態へ遷移する
    /// </summary>
    protected bool TryChangeByJumpCommand()
    {
        if (!context.Commands.TryConsumeJump())
        {
            return false;
        }

        if (!context.TryJump())
        {
            return false;
        }

        bodyStateMachine.ChangeToAirborneState();
        return true;
    }

    /// <summary>
    /// 現在の移動入力に応じて地上状態を切り替える
    /// </summary>
    protected void ChangeGroundedStateByInput()
    {
        if (!HasMoveInput())
        {
            bodyStateMachine.ChangeToGroundedIdleState();
            return;
        }

        bodyStateMachine.ChangeToGroundedMoveState();
    }
}
