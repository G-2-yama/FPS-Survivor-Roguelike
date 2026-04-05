using UnityEngine;

public class PlayerIdleState : PlayerMoveState
{
    public PlayerIdleState(PlayerController controller,
                           StateMachine<PlayerMoveState> moveStateMachine)
        : base(controller, moveStateMachine) { }

    public override void Enter()
    {
        Debug.Log("Idle Stateに入りました");
    }

    /// <summary>
    /// 入力が発生したら歩行状態へ遷移
    /// </summary>
    public override void Update()
    {
        if (!controller.IsGrounded)
        {
            moveStateMachine.ChangeState(new PlayerAirState(controller, moveStateMachine));
            return;
        }

        if (controller.ConsumeJumpRequest())
        {
            controller.Jump();
            moveStateMachine.ChangeState(new PlayerAirState(controller, moveStateMachine    ));
            return;
        }

        if (controller.MoveInput == Vector2.zero)
        {
            return;
        } 

        if (controller.IsSprinting)
        {
            moveStateMachine.ChangeState(new PlayerSprintState(controller, moveStateMachine));
        }
        else
        {
            moveStateMachine.ChangeState(new PlayerWalkState(controller, moveStateMachine));
        }
    }
}
