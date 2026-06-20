/// <summary>
/// 空中にいる身体状態
/// </summary>
public class PlayerAirborneState : PlayerBodyState
{
    public override PlayerBodyStateId StateId => PlayerBodyStateId.Airborne;

    public PlayerAirborneState(
        PlayerContext context,
        PlayerBodyStateMachine bodyStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, bodyStateMachine, actionStateMachine) { }

    public override void Update()
    {
        if (TryChangeByCrouchActionToFastFall())
        {
            return;
        }

        if (TryChangeByDashCommand())
        {
            return;
        }

        if (TryChangeByJumpCommand())
        {
            return;
        }

        if (context.Motor.IsGrounded())
        {
            ChangeGroundedStateByInput();
        }
    }
}
