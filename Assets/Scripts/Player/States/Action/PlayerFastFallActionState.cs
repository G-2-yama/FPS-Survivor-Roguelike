using UnityEngine;

/// <summary>
/// 空中で下方向へ急加速して着地を早める動作ステート
/// </summary>
public class PlayerFastFallActionState : PlayerActionState
{
    public PlayerFastFallActionState(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
        : base(context, actionStateMachine) { }

    public bool CanEnter()
    {
        return !context.Motor.IsGrounded();
    }

    public override void Enter()
    {
        context.Motor.VerticalVelocity = Mathf.Min(
            context.Motor.VerticalVelocity,
            -context.Config.FastFallEntrySpeed);
    }

    public override void Update()
    {
        if (context.Motor.IsGrounded())
        {
            if (context.Controls.CrouchHeld && actionStateMachine.TryChangeToSlideActionState())
            {
                return;
            }

            actionStateMachine.ChangeToNoActionState();
            return;
        }

        float extraDownwardVelocity = context.Config.FastFallAcceleration * Time.deltaTime;
        context.Motor.VerticalVelocity = Mathf.Max(
            context.Motor.VerticalVelocity - extraDownwardVelocity,
            -context.Config.FastFallTerminalSpeed);
    }
}
