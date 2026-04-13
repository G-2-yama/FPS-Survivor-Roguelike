using UnityEngine;

/// <summary>
/// 地上で移動入力がない状態属性ステート。
/// </summary>
public class PlayerIdleStatusState : PlayerStatusState
{
    /// <summary>
    /// 待機状態を初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="statusStateMachine">状態属性ステートマシン。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerIdleStatusState(
        PlayerContext context,
        PlayerStatusStateMachine statusStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, statusStateMachine, actionStateMachine) { }

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
            return;
        }

        TransitionToGroundStatusByInput();
    }
}
