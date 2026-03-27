using UnityEngine;

public class PlayerWalkState : PlayerMoveState
{
    public PlayerWalkState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Walk Stateに入りました");
    }

    /// <summary>
    /// 歩行状態の更新を行い、入力がない場合は待機状態へ遷移
    /// </summary>
    public override void Update()
    {
        if (controller.MoveInput == Vector2.zero)
        {
            controller.StateMachine.ChangeState(new PlayerIdleState(controller));
            return;
        }
        else if (controller.IsSprinting)
        {
            controller.StateMachine.ChangeState(new PlayerSprintState(controller));
            return;
        }

        controller.Move();
    }

}
