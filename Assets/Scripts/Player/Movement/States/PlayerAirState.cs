using UnityEngine;

/// <summary>
/// 空中にいる間の移動ステート。
/// </summary>
public class PlayerAirState : PlayerMoveState
{
    /// <summary>
    /// 空中ステートを初期化する。
    /// </summary>
    /// <param name="controller">プレイヤー制御本体。</param>
    /// <param name="moveStateMachine">移動サブステートマシン。</param>
    public PlayerAirState(PlayerController controller,
                          StateMachine<PlayerMoveState> moveStateMachine)
        : base(controller, moveStateMachine) { }

    public override void Enter()
    {
        Debug.Log("Air Stateに入りました");
    }

    /// <summary>
    /// 空中状態では重力と空中制御を適用し、接地したら入力に応じて地上状態へ遷移
    /// </summary>
    public override void Update()
    {
        if (controller.IsGrounded)
        {
            TransitionToGroundMoveStateByInput();
            return;
        }

        controller.ConsumeJumpRequest();
    }
}
