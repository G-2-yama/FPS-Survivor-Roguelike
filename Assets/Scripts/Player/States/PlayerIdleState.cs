using UnityEngine;

public class PlayerIdleState : PlayerMoveState
{
    public PlayerIdleState(PlayerController controller) : base(controller) { }

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
            controller.StateMachine.ChangeState(new PlayerAirState(controller));
            return;
        }

        if (controller.MoveInput == Vector2.zero)
        {
            return;
        } 

        if (controller.IsSprinting)
        {
            controller.StateMachine.ChangeState(new PlayerSprintState(controller));
        }
        else
        {
            controller.StateMachine.ChangeState(new PlayerWalkState(controller));
        }
    }
}
