using UnityEngine;

public class PlayerAirState : PlayerMoveState
{
    public PlayerAirState(PlayerController controller,
                          StateMachine<PlayerMoveState> moveStateMachine)
        : base(controller, moveStateMachine) { }

    public override void Enter()
    {
        Debug.Log("Air Stateに入りました");
    }

    /// <summary>
    /// 空中状態では重力と空中制御を適用し、接地したら入力に応じて地上状態へ遷移
    /// </summary>
    public override void Update()
    {
        if (controller.IsGrounded)
        {
            if (controller.MoveInput == Vector2.zero)
            {
                moveStateMachine.ChangeState(new PlayerIdleState(controller, moveStateMachine));
            }
            else if (controller.IsSprinting)
            {
                moveStateMachine.ChangeState(new PlayerSprintState(controller, moveStateMachine));
            }
            else
            {
                moveStateMachine.ChangeState(new PlayerWalkState(controller, moveStateMachine));
            }

            return;
        }

        controller.ConsumeJumpRequest();
    }
}
