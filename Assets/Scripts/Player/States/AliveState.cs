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

        // 反動処理
        Vector2 recoil = Vector2.zero;
        recoil += controller.LeftWeaponController.WeaponRecoil.Update(Time.deltaTime);
        recoil += controller.RightWeaponController.WeaponRecoil.Update(Time.deltaTime);
        controller.Look.AddRecoil(recoil);

        // 移動処理
        controller.Mover.Move(
            controller.MoveInput,
            controller.IsSprinting
        );

    }

    public override void Exit()
    {
        moveStateMachine.ChangeState(null);
    }
}
