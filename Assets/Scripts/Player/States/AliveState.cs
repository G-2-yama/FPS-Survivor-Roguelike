using UnityEngine;

public class AliveState : PlayerState
{
    private PlayerMoveStateMachine moveStateMachine;

    public AliveState(PlayerController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Enter Alive State");
        moveStateMachine = new PlayerMoveStateMachine(controller);
    }

    public override void Update()
    {
        moveStateMachine.Update();

        controller.Look.ApplyLook(controller.LookInput);

        Vector2 recoil = controller.WeaponController
            .WeaponRecoil.Update(Time.deltaTime);

        controller.Look.AddRecoil(recoil);

        if (moveStateMachine.CurrentMoveState is PlayerDashState)
        {
            return;
        }

        controller.Mover.Move(
            controller.MoveInput,
            controller.IsJumpHeld,
            Time.deltaTime
        );
    }

    public override void Exit()
    {
        moveStateMachine.ChangeState(null);
    }
}
