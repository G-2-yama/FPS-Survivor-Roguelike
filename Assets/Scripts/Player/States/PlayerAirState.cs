using UnityEngine;

public class PlayerAirState : PlayerMoveState
{
    public PlayerAirState(PlayerController controller) : base(controller) { }

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
                controller.StateMachine.ChangeState(new PlayerIdleState(controller));
            }
            else if (controller.IsSprinting)
            {
                controller.StateMachine.ChangeState(new PlayerSprintState(controller));
            }
            else
            {
                controller.StateMachine.ChangeState(new PlayerWalkState(controller));
            }

            return;
        }

        controller.ConsumeJumpRequest();

        controller.Move();
    }
}
