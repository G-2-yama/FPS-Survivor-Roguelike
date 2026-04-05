using UnityEngine;

public class PlayerWalkState : PlayerMoveState
{
    public PlayerWalkState(PlayerController controller,
                           StateMachine<PlayerMoveState> moveStateMachine)
        : base(controller, moveStateMachine) { }

    public override void Enter()
    {
        Debug.Log("Walk Stateに入りました");
    }

    /// <summary>
    /// 歩行状態の更新を行い、入力がない場合は待機状態へ遷移
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
        else if (controller.IsSprinting)
        {
            moveStateMachine.ChangeState(new PlayerSprintState(controller, moveStateMachine));
            return;
        }
    }

}
