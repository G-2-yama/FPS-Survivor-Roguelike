using UnityEngine;

/// <summary>
/// 通常移動中の移動ステート。
/// </summary>
public class PlayerWalkState : PlayerMoveState
{
    /// <summary>
    /// 歩行ステートを初期化する。
    /// </summary>
    /// <param name="controller">プレイヤー制御本体。</param>
    /// <param name="moveStateMachine">移動サブステートマシン。</param>
    public PlayerWalkState(PlayerController controller,
                           PlayerMoveStateMachine moveStateMachine)
        : base(controller, moveStateMachine) { }

    public override void Enter()
    {
        Debug.Log("Walk Stateに入りました");
    }

    /// <summary>
    /// 歩行状態の更新を行い、入力がない場合は待機状態へ遷移
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
            moveStateMachine.ChangeIdleState();
            return;
        }

        if (controller.IsSprinting)
        {
            moveStateMachine.ChangeSprintState();
            return;
        }
    }

}
