/// <summary>
/// 死亡して操作を受け付けない身体状態
/// </summary>
public class PlayerDeadState : PlayerBodyState
{
    public override PlayerBodyStateId StateId => PlayerBodyStateId.Dead;

    public PlayerDeadState(
        PlayerContext context,
        PlayerBodyStateMachine bodyStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, bodyStateMachine, actionStateMachine) { }
}
