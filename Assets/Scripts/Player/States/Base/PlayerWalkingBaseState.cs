using UnityEngine;

/// <summary>
/// 地上で移動入力がある基底ステート。
/// </summary>
public class PlayerWalkingBaseState : PlayerBaseState
{
    /// <summary>
    /// 歩行状態を初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="baseStateMachine">基底ステートマシン。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerWalkingBaseState(
        PlayerContext context,
        PlayerBaseStateMachine baseStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, baseStateMachine, actionStateMachine) { }

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

        if (TryTransitionToUngroundedState())
        {
            return;
        }

        if (TryTransitionByJumpRequest())
        {
            return;
        }

        if (!HasMoveInput())
        {
            baseStateMachine.ChangeIdleState();
            return;
        }
    }
}
