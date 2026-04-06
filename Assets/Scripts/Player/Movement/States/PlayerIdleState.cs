using UnityEngine;

/// <summary>
/// 入力待機中の移動ステート。
/// </summary>
public class PlayerIdleState : PlayerMoveState
{
    /// <summary>
    /// 待機ステートを初期化する。
    /// </summary>
    /// <param name="controller">プレイヤー制御本体。</param>
    /// <param name="moveStateMachine">移動サブステートマシン。</param>
    public PlayerIdleState(PlayerController controller,
                           PlayerMoveStateMachine moveStateMachine)
        : base(controller, moveStateMachine) { }

    public override void Enter()
    {
        Debug.Log("Idle Stateに入りました");
    }

    /// <summary>
    /// 入力が発生したら歩行状態へ遷移
    /// </summary>
    public override void Update()
    {
        if (TryTransitionToAirState())
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

        TransitionToGroundMoveStateByInput();
    }
}
