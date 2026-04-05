using Unity.VisualScripting;

public class PlayerMoveStateMachine : StateMachine<PlayerMoveState>
{
    private PlayerMoveState IdleState;
    public PlayerMoveState AirState => airState;

    private PlayerMoveState airState;
    public PlayerMoveState WalkState => walkState;

    private PlayerMoveState walkState;
    public PlayerMoveState SprintState => sprintState;

    private PlayerMoveState sprintState;
    public PlayerMoveState CurrentMoveState => currentState;

    public PlayerMoveStateMachine(PlayerController controller)
    {
        IdleState = new PlayerIdleState(controller, this);
        airState = new PlayerAirState(controller, this);
        walkState = new PlayerWalkState(controller, this);
        sprintState = new PlayerSprintState(controller, this);

        ChangeState(IdleState);
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
        ChangeState(IdleState);
    }
}