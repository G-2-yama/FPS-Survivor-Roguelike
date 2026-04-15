using UnityEngine;

/// <summary>
/// 地上で移動入力がない基底ステート。
/// </summary>
public class PlayerIdleBaseState : PlayerBaseState
{
    /// <summary>
    /// 待機状態を初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="baseStateMachine">基底ステートマシン。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerIdleBaseState(
        PlayerContext context,
        PlayerBaseStateMachine baseStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, baseStateMachine, actionStateMachine) { }

    /// <summary>
    /// 待機状態に入ったことをログ出力する
    /// </summary>
    public override void Enter()
    {
        Debug.Log("Idle Stateに入りました");
    }

    /// <summary>
    /// 入力が発生したら歩行状態へ遷移
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
            return;
        }

        TransitionToGroundStatusByInput();
    }
}
