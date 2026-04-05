using UnityEngine;

public class PlayerSprintState : PlayerMoveState
{
    public PlayerSprintState(PlayerController controller,
                             StateMachine<PlayerMoveState> moveStateMachine)
        : base(controller, moveStateMachine) { }

    public override void Enter()
    {
        Debug.Log("Sprint Stateに入りました");
    }

    /// <summary>
    /// スプリント状態の更新を行い、入力がない場合は待機状態へ遷移
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
            moveStateMachine.ChangeState(new PlayerAirState(controller, moveStateMachine));
            return;
        }

        if (controller.MoveInput == Vector2.zero)
        {
            moveStateMachine.ChangeState(new PlayerIdleState(controller, moveStateMachine));
            return;
        }
        else if (!controller.IsSprinting)
        {
            moveStateMachine.ChangeState(new PlayerWalkState(controller, moveStateMachine));
            return;
        }
    }

}
