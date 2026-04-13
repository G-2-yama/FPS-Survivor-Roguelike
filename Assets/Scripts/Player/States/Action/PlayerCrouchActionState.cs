/// <summary>
/// その場でしゃがむ動作ステート
/// </summary>
public class PlayerCrouchActionState : PlayerActionState
{
    /// <summary>
    /// しゃがみ動作ステートを初期化する
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト</param>
    /// <param name="actionStateMachine">動作ステートマシン</param>
    public PlayerCrouchActionState(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
        : base(context, actionStateMachine) { }
}
