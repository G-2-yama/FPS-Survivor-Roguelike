using UnityEngine;

public class PlayerDashState : PlayerMoveState
{
    private Vector3 dashDirection;
    private float dashTimer;
    private float cooldownTimer;

    public PlayerDashState(PlayerController controller,
                           PlayerMoveStateMachine moveStateMachine)
        : base(controller, moveStateMachine) { }

    public bool CanEnter()
    {
        return cooldownTimer <= 0f
            && controller.Mover.TryGetMoveDirection(controller.MoveInput, out _);
    }

    public void UpdateCooldown(float deltaTime)
    {
        if (cooldownTimer <= 0f)
        {
            return;
        }

        cooldownTimer = Mathf.Max(0f, cooldownTimer - deltaTime);
    }

    public override void Enter()
    {
        if (!controller.Mover.TryGetMoveDirection(controller.MoveInput, out dashDirection))
        {
            TransitionToGroundMoveStateByInput();
            return;
        }

        dashTimer = controller.Player.Config.DashDuration;
        controller.Mover.ClearHorizontalVelocity();
    }

    public override void Update()
    {
        float dashStep = Mathf.Min(Time.deltaTime, dashTimer);
        controller.Mover.MoveDash(dashDirection, dashStep);
        dashTimer -= dashStep;

        if (dashTimer > 0f)
        {
            return;
        }

        if (!controller.IsGrounded)
        {
            moveStateMachine.ChangeAirState();
            return;
        }

        TransitionToGroundMoveStateByInput();
    }

    public override void Exit()
    {
        dashTimer = 0f;
        dashDirection = Vector3.zero;
        cooldownTimer = controller.Player.Config.DashCooldown;
    }
}
