using UnityEngine;

/// <summary>
/// 身体状態と一時アクション状態をまとめて更新する。
/// 身体状態は継続的なプレイヤー状態、Action は割り込み動作として分けている。
/// </summary>
public class PlayerStateCoordinator
{
    /// <summary>
    /// 状態更新で参照するプレイヤー制御コンテキスト
    /// </summary>
    private PlayerContext context;

    /// <summary>
    /// 継続的な身体状態を管理するステートマシン
    /// </summary>
    private PlayerBodyStateMachine bodyStateMachine;

    /// <summary>
    /// 一時アクション状態を管理するステートマシン
    /// </summary>
    private PlayerActionStateMachine actionStateMachine;

    /// <summary>
    /// 現在の身体状態ID
    /// </summary>
    public PlayerBodyStateId CurrentBodyStateId => bodyStateMachine.CurrentStateId;

    /// <summary>
    /// 2種類のプレイヤーステートマシンを初期化する
    /// </summary>
    public PlayerStateCoordinator(PlayerContext context)
    {
        this.context = context;
        actionStateMachine = new PlayerActionStateMachine(context);
        bodyStateMachine = new PlayerBodyStateMachine(context, actionStateMachine);
    }

    /// <summary>
    /// 状態の更新処理を実行する
    /// </summary>
    public void Update(float deltaTime)
    {
        if (CurrentBodyStateId == PlayerBodyStateId.Dead)
        {
            bodyStateMachine.Update();
            return;
        }

        actionStateMachine.Update(deltaTime);

        if (!actionStateMachine.IsBlockingNormalMovement)
        {
            bodyStateMachine.Update();
        }


        Vector2 recoil = context.WeaponControllerManager.GetTotalRecoil(deltaTime);
        context.Look.ApplyView(context.Controls.LookInput, recoil);

        if (actionStateMachine.IsBlockingNormalMovement)
        {
            return;
        }

        context.Locomotion.Move(
            context.Controls.MoveInput,
            context.Controls.JumpHeld,
            deltaTime);
    }

    /// <summary>
    /// 死亡状態へ遷移する
    /// </summary>
    public void ChangeToDeadState()
    {
        actionStateMachine.ChangeToNoActionState();
        bodyStateMachine.ChangeToDeadState();
    }
}
