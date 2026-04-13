/// <summary>
/// 特殊な動作を行っていない動作ステート
/// </summary>
public class PlayerNoActionState : PlayerActionState
{
    /// <summary>
    /// 特殊動作なしステートを初期化する
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト</param>
    /// <param name="actionStateMachine">動作ステートマシン</param>
    public PlayerNoActionState(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
        : base(context, actionStateMachine) { }
}
