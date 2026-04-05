using UnityEngine;

/// <summary>
/// スプリント中の移動ステート。
/// </summary>
public class PlayerSprintState : PlayerMoveState
{
    /// <summary>
    /// スプリントステートを初期化する。
    /// </summary>
    /// <param name="controller">プレイヤー制御本体。</param>
    /// <param name="moveStateMachine">移動サブステートマシン。</param>
    public PlayerSprintState(PlayerController controller,
                             StateMachine<PlayerMoveState> moveStateMachine)
        : base(controller, moveStateMachine) { }

    public override void Enter()
    {
        Debug.Log("Sprint Stateに入りました");
    }

    /// <summary>
    /// スプリント状態の更新を行い、入力がない場合は待機状態へ遷移
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
            moveStateMachine.ChangeState(new PlayerIdleState(controller, moveStateMachine));
            return;
        }

        if (!controller.IsSprinting)
        {
            moveStateMachine.ChangeState(new PlayerWalkState(controller, moveStateMachine));
            return;
        }
    }

}
