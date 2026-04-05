using Unity.VisualScripting;

public class PlayerMoveStateMachine : StateMachine<PlayerMoveState>
{
    private PlayerMoveState idleState;
    public PlayerMoveState IdleStatePublic => idleState;

    private PlayerMoveState airState;
    public PlayerMoveState AirState => airState;

    private PlayerMoveState walkState;
    public PlayerMoveState WalkState => walkState;

    private PlayerMoveState sprintState;
    public PlayerMoveState SprintState => sprintState;
    
    public PlayerMoveState CurrentMoveState => currentState;

    public PlayerMoveStateMachine(PlayerController controller)
    {
        idleState = new PlayerIdleState(controller, this);
        airState = new PlayerAirState(controller, this);
        walkState = new PlayerWalkState(controller, this);
        sprintState = new PlayerSprintState(controller, this);

        ChangeState(idleState);
    }

    /// <summary>
    /// 空中状態に遷移
    /// </summary>
    public void ChangeAirState()
    {
        ChangeState(airState);
    }

    /// <summary>
    /// 歩き状態に遷移
    /// </summary>
    public void ChangeWalkState()
    {
        ChangeState(walkState);
    }

    /// <summary>
    /// 走り状態に遷移
    /// </summary>
    public void ChangeSprintState()
    {
        ChangeState(sprintState);
    }

    /// <summary>
    /// 待機状態に遷移
    /// </summary>
    public void ChangeIdleState()
    {
        ChangeState(idleState);
    }
}