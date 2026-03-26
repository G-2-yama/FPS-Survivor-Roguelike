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
        if (model.moveInput != Vector2.zero)
        {
            controller.StateMachine.ChangeState(new PlayerWalkState(controller));
        }
    }
}
