using UnityEngine;

/// <summary>
/// 地上で移動入力がある状態属性ステート。
/// </summary>
public class PlayerWalkingStatusState : PlayerStatusState
{
    /// <summary>
    /// 歩行状態を初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="statusStateMachine">状態属性ステートマシン。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerWalkingStatusState(
        PlayerContext context,
        PlayerStatusStateMachine statusStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, statusStateMachine, actionStateMachine) { }

    /// <summary>
    /// 歩行状態に入ったことをログ出力する
    /// </summary>
    public override void Enter()
    {
        Debug.Log("Walk Stateに入りました");
    }

    /// <summary>
    /// 歩行状態の更新を行い、入力がない場合は待機状態へ遷移
    /// </summary>
    public override void Update()
    {
        if (TryTransitionByDashRequest())
        {
            return;
        }

        if (TryTransitionToAirborneState())
        {
            return;
        }

        if (TryTransitionByJumpRequest())
        {
            return;
        }

        if (!HasMoveInput())
        {
            statusStateMachine.ChangeIdleState();
            return;
        }
    }
}
