using UnityEngine;

/// <summary>
/// 空中にいる状態属性ステート。
/// </summary>
public class PlayerAirborneStatusState : PlayerStatusState
{
    /// <summary>
    /// この状態属性ステートは空中状態を表す
    /// </summary>
    public override bool IsAirborne => true;

    /// <summary>
    /// 空中状態を初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="statusStateMachine">状態属性ステートマシン。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerAirborneStatusState(
        PlayerContext context,
        PlayerStatusStateMachine statusStateMachine,
        PlayerActionStateMachine actionStateMachine)
        : base(context, statusStateMachine, actionStateMachine) { }

    /// <summary>
    /// 空中状態に入ったことをログ出力する
    /// </summary>
    public override void Enter()
    {
        Debug.Log("Air Stateに入りました");
    }

    /// <summary>
    /// 空中状態では接地したら入力に応じて地上状態へ遷移
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
