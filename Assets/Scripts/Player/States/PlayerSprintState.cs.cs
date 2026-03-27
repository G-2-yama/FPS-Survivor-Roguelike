using UnityEngine;

public class PlayerSprintState : PlayerMoveState
{
    public PlayerSprintState(PlayerController controller) : base(controller) { }

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
            controller.StateMachine.ChangeState(new PlayerAirState(controller));
            return;
        }

        if (controller.ConsumeJumpRequest())
        {
            controller.Jump();
            controller.StateMachine.ChangeState(new PlayerAirState(controller));
            return;
        }

        if (controller.MoveInput == Vector2.zero)
        {
            controller.StateMachine.ChangeState(new PlayerIdleState(controller));
            return;
        }
        else if (!controller.IsSprinting)
        {
            controller.StateMachine.ChangeState(new PlayerWalkState(controller));
            return;
        }
    }

}
