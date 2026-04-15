using UnityEngine;

/// <summary>
/// 接地していない基底ステート。
/// </summary>
public class PlayerUngroundedBaseState : PlayerBaseState
{
    /// <summary>
    /// この基底ステートは非接地状態を表す
    /// </summary>
    public override bool IsUngrounded => true;

    /// <summary>
    /// 非接地状態を初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="baseStateMachine">基底ステートマシン。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerUngroundedBaseState(
        PlayerContext context,
        PlayerBaseStateMachine baseStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, baseStateMachine, actionStateMachine) { }

    /// <summary>
    /// 非接地状態に入ったことをログ出力する
    /// </summary>
    public override void Enter()
    {
        Debug.Log("Ungrounded Stateに入りました");
    }

    /// <summary>
    /// 非接地状態では接地したら入力に応じて地上状態へ遷移
    /// </summary>
    public override void Update()
    {
        if (TryTransitionByDashRequest())
        {
            return;
        }

        if (TryTransitionByJumpRequest())
        {
            return;
        }

        if (context.Motor.IsGrounded())
        {
            TransitionToGroundStatusByInput();
            return;
        }
    }
}
