/// <summary>
/// プレイヤーの状態属性ステートを管理するステートマシン
/// </summary>
public class PlayerStatusStateMachine : StateMachine<PlayerStatusState>
{
    /// <summary>
    /// 地上で移動入力がない状態
    /// </summary>
    private PlayerStatusState idleState;
    public PlayerStatusState IdleState => idleState;

    /// <summary>
    /// 地上で移動入力がある状態
    /// </summary>
    private PlayerStatusState walkingState;
    public PlayerStatusState WalkingState => walkingState;

    /// <summary>
    /// 接地していない状態
    /// </summary>
    private PlayerStatusState airborneState;
    public PlayerStatusState AirborneState => airborneState;

    /// <summary>
    /// 死亡している状態
    /// </summary>
    private PlayerStatusState deadState;
    public PlayerStatusState DeadState => deadState;

    /// <summary>
    /// 現在の状態属性ステート
    /// </summary>
    public PlayerStatusState CurrentStatusState => currentState;

    /// <summary>
    /// 現在の状態属性が空中状態かどうか
    /// </summary>
    public bool IsAirborne => currentState?.IsAirborne ?? false;

    /// <summary>
    /// 現在の状態属性が地上状態かどうか
    /// </summary>
    public bool IsGrounded => !IsAirborne && !IsDead;

    /// <summary>
    /// 現在の状態属性が死亡状態かどうか
    /// </summary>
    public bool IsDead => currentState?.IsDead ?? false;

    /// <summary>
    /// 状態属性ステートを生成し、待機状態へ遷移する
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト</param>
    /// <param name="actionStateMachine">動作ステートマシン</param>
    public PlayerStatusStateMachine(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
    {
        idleState = new PlayerIdleStatusState(context, this, actionStateMachine);
        walkingState = new PlayerWalkingStatusState(context, this, actionStateMachine);
        airborneState = new PlayerAirborneStatusState(context, this, actionStateMachine);
        deadState = new PlayerDeadStatusState(context, this, actionStateMachine);

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
    /// 空中状態に遷移
    /// </summary>
    public void ChangeAirborneState()
    {
        ChangeState(airborneState);
    }

    /// <summary>
    /// 死亡状態に遷移
    /// </summary>
    public void ChangeDeadState()
    {
        ChangeState(deadState);
    }
}
