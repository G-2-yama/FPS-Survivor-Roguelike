/// <summary>
/// プレイヤーの継続的な身体状態を管理するステートマシン
/// </summary>
public class PlayerBodyStateMachine : StateMachine<PlayerBodyState>
{
    private PlayerGroundedIdleState groundedIdleState;
    private PlayerGroundedMoveState groundedMoveState;
    private PlayerAirborneState airborneState;
    private PlayerDeadState deadState;

    /// <summary>
    /// 現在の身体状態ID
    /// </summary>
    public PlayerBodyStateId CurrentStateId => currentState.StateId;

    /// <summary>
    /// 身体状態を生成し、初期状態へ遷移する
    /// </summary>
    public PlayerBodyStateMachine(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
    {
        groundedIdleState = new PlayerGroundedIdleState(context, this, actionStateMachine);
        groundedMoveState = new PlayerGroundedMoveState(context, this, actionStateMachine);
        airborneState = new PlayerAirborneState(context, this, actionStateMachine);
        deadState = new PlayerDeadState(context, this, actionStateMachine);

        ChangeState(groundedIdleState);
    }

    public void ChangeToGroundedIdleState()
    {
        ChangeState(groundedIdleState);
    }

    public void ChangeToGroundedMoveState()
    {
        ChangeState(groundedMoveState);
    }

    public void ChangeToAirborneState()
    {
        ChangeState(airborneState);
    }

    public void ChangeToDeadState()
    {
        ChangeState(deadState);
    }
}
