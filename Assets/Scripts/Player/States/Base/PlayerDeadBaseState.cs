using UnityEngine;

/// <summary>
/// プレイヤーが死亡している基底ステート。
/// </summary>
public class PlayerDeadBaseState : PlayerBaseState
{
    /// <summary>
    /// この基底ステートは死亡状態を表す
    /// </summary>
    public override bool IsDead => true;

    /// <summary>
    /// 死亡状態を初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="baseStateMachine">基底ステートマシン。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerDeadBaseState(
        PlayerContext context,
        PlayerBaseStateMachine baseStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, baseStateMachine, actionStateMachine) { }

    /// <summary>
    /// 死亡状態に入ったことをログ出力する
    /// </summary>
    public override void Enter()
    {
        Debug.Log("Death Stateに入りました");
    }

    /// <summary>
    /// 死亡中は行動せず待機
    /// </summary>
    public override void Update()
    {
    }
}
