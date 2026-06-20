/// <summary>
/// 地上で移動している身体状態
/// </summary>
public class PlayerGroundedMoveState : PlayerBodyState
{
    public override PlayerBodyStateId StateId => PlayerBodyStateId.GroundedMove;

    public PlayerGroundedMoveState(
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
            bodyStateMachine.ChangeToGroundedIdleState();
        }
    }
}
