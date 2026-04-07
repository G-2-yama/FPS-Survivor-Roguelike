using UnityEngine;

public class AliveState : PlayerState
{
    private PlayerMoveStateMachine moveStateMachine;

    public AliveState(PlayerController controller) : base(controller) { }


    public override void Enter()
    {
        Debug.Log("Alive Stateに入りました");

        moveStateMachine = new PlayerMoveStateMachine(controller);
    }

    public override void Update()
    {
        // MoveState更新
        moveStateMachine.Update();

        // 視点処理
        controller.Look.ApplyLook(controller.LookInput);

        Vector2 recoil = controller.WeaponController
            .WeaponRecoil.Update(Time.deltaTime);

        controller.Look.AddRecoil(recoil);
    

        // 移動処理
        controller.Mover.Move(
            controller.MoveInput,
            controller.IsSprinting,
            Time.deltaTime
        );

    }

    public override void Exit()
    {
        moveStateMachine.ChangeState(null);
    }
}
