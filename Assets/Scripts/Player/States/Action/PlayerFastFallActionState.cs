using UnityEngine;

/// <summary>
/// 空中で下方向へ急加速して着地を早める動作ステート
/// </summary>
public class PlayerFastFallActionState : PlayerActionState
{
    /// <summary>急降下の制御に必要な共有状態を関連付ける</summary>
    public PlayerFastFallActionState(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
        : base(context, actionStateMachine) { }

    /// <summary>空中で急降下を開始できるか確認する</summary>
    public bool CanEnter()
    {
        return !context.Motor.IsGrounded();
    }

    /// <summary>急降下の開始速度と演出を設定する</summary>
    public override void Enter()
    {
        context.Motor.VerticalVelocity = Mathf.Min(
            context.Motor.VerticalVelocity,
            -context.ControlSettings.FastFallEntrySpeed);
        context.ActionTrail?.Begin(PlayerActionTrail.ActionType.FastFall);
        context.ActionConcentrationLine?.Begin(PlayerActionConcentrationLine.ActionType.FastFall);
    }

    /// <summary>下向きの追加加速度を適用し、接地したら終了する</summary>
    public override void Update()
    {
        if (context.Motor.IsGrounded())
        {
            if (context.Controls.CrouchHeld && actionStateMachine.TryChangeToSlideActionState())
            {
                return;
            }

            actionStateMachine.ChangeToNoActionState();
            return;
        }

        float extraDownwardVelocity = context.ControlSettings.FastFallAcceleration * Time.deltaTime;
        context.Motor.VerticalVelocity = Mathf.Max(
            context.Motor.VerticalVelocity - extraDownwardVelocity,
            -context.ControlSettings.FastFallTerminalSpeed);
    }

    /// <summary>急降下の演出を終了する</summary>
    public override void Exit()
    {
        context.ActionTrail?.End(PlayerActionTrail.ActionType.FastFall);
        context.ActionConcentrationLine?.End(PlayerActionConcentrationLine.ActionType.FastFall);
    }
}
