/// <summary>
/// プレイヤーの基底ステートを管理するステートマシン
/// </summary>
public class PlayerBaseStateMachine : StateMachine<PlayerBaseState>
{
    /// <summary>
    /// 地上で移動入力がない状態
    /// </summary>
    private PlayerBaseState idleState;
    public PlayerBaseState IdleState => idleState;

    /// <summary>
    /// 地上で移動入力がある状態
    /// </summary>
    private PlayerBaseState walkingState;
    public PlayerBaseState WalkingState => walkingState;

    /// <summary>
    /// 接地していない状態
    /// </summary>
    private PlayerBaseState ungroundedState;
    public PlayerBaseState UngroundedState => ungroundedState;

    /// <summary>
    /// 死亡している状態
    /// </summary>
    private PlayerBaseState deadState;
    public PlayerBaseState DeadState => deadState;

    /// <summary>
    /// 現在の基底ステート
    /// </summary>
    public PlayerBaseState CurrentBaseState => currentState;

    /// <summary>
    /// 現在の基底ステートが非接地状態かどうか
    /// </summary>
    public bool IsUngrounded => currentState?.IsUngrounded ?? false;

    /// <summary>
    /// 現在の基底ステートが地上状態かどうか
    /// </summary>
    public bool IsGrounded => !IsUngrounded && !IsDead;

    /// <summary>
    /// 現在の基底ステートが死亡状態かどうか
    /// </summary>
    public bool IsDead => currentState?.IsDead ?? false;

    /// <summary>
    /// 基底ステートを生成し、待機状態へ遷移する
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト</param>
    /// <param name="actionStateMachine">動作ステートマシン</param>
    public PlayerBaseStateMachine(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
    {
        idleState = new PlayerIdleBaseState(context, this, actionStateMachine);
        walkingState = new PlayerWalkingBaseState(context, this, actionStateMachine);
        ungroundedState = new PlayerUngroundedBaseState(context, this, actionStateMachine);
        deadState = new PlayerDeadBaseState(context, this, actionStateMachine);

        ChangeState(idleState);
    }

    /// <summary>
    /// 待機状態に遷移
    /// </summary>
    public void ChangeIdleState()
    {
        ChangeState(idleState);
    }

    /// <summary>
    /// 歩行状態に遷移
    /// </summary>
    public void ChangeWalkingState()
    {
        ChangeState(walkingState);
    }

    /// <summary>
    /// 非接地状態に遷移
    /// </summary>
    public void ChangeUngroundedState()
    {
        ChangeState(ungroundedState);
    }

    /// <summary>
    /// 死亡状態に遷移
    /// </summary>
    public void ChangeDeadState()
    {
        ChangeState(deadState);
    }
}
