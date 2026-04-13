/// <summary>
/// スライディングする動作ステート。
/// </summary>
public class PlayerSlideActionState : PlayerActionState
{
    /// <summary>
    /// スライディング動作ステートを初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerSlideActionState(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
        : base(context, actionStateMachine) { }
}
