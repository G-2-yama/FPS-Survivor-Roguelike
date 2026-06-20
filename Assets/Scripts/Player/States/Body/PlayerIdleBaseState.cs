/// <summary>
/// 地上で停止している身体状態
/// </summary>
public class PlayerGroundedIdleState : PlayerBodyState
{
    public override PlayerBodyStateId StateId => PlayerBodyStateId.GroundedIdle;

    public PlayerGroundedIdleState(
        PlayerContext context,
        PlayerBodyStateMachine bodyStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, bodyStateMachine, actionStateMachine) { }

    public override void Update()
    {
        if (TryChangeByQueuedLandingSlide())
        {
            return;
        }

        if (TryChangeByCrouchActionToSlide())
        {
            return;
        }

        if (TryChangeByDashCommand())
        {
            return;
        }

        if (TryChangeToAirborneState())
        {
            return;
        }

        if (TryChangeByJumpCommand())
        {
            return;
        }

        if (!HasMoveInput())
        {
            return;
        }

        ChangeGroundedStateByInput();
    }
}
