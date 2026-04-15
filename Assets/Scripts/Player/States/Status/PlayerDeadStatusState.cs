using UnityEngine;

/// <summary>
/// プレイヤーが死亡している状態属性ステート。
/// </summary>
public class PlayerDeadStatusState : PlayerStatusState
{
    /// <summary>
    /// この状態属性ステートは死亡状態を表す
    /// </summary>
    public override bool IsDead => true;

    /// <summary>
    /// 死亡状態を初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="statusStateMachine">状態属性ステートマシン。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerDeadStatusState(
        PlayerContext context,
        PlayerStatusStateMachine statusStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, statusStateMachine, actionStateMachine) { }

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
