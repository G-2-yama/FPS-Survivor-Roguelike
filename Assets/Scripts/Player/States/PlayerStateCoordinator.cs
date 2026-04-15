using UnityEngine;

/// <summary>
/// プレイヤーの状態属性ステートと動作ステートをまとめて更新する管理クラス
/// </summary>
public class PlayerStateCoordinator
{
    /// <summary>
    /// 状態更新で参照するプレイヤー制御コンテキスト
    /// </summary>
    private PlayerContext context;

    /// <summary>
    /// Idle/Walking/Airborne/Deadを管理する状態属性ステートマシン
    /// </summary>
    private PlayerStatusStateMachine statusStateMachine;
    public PlayerStatusStateMachine StatusStateMachine => statusStateMachine;

    /// <summary>
    /// NoAction/Dashなどの一時動作を管理する動作ステートマシン
    /// </summary>
    private PlayerActionStateMachine actionStateMachine;
    public PlayerActionStateMachine ActionStateMachine => actionStateMachine;

    /// <summary>
    /// 現在の状態属性が死亡状態かどうか
    /// </summary>
    public bool IsDead => statusStateMachine.IsDead;

    /// <summary>
    /// 2種類のプレイヤーステートマシンを初期化する
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト</param>
    public PlayerStateCoordinator(PlayerContext context)
    {
        this.context = context;
        actionStateMachine = new PlayerActionStateMachine(context);
        statusStateMachine = new PlayerStatusStateMachine(context, actionStateMachine);
    }

    /// <summary>
    /// 状態の更新処理を実行する
    /// </summary>
    public void Update()
    {
        if (IsDead)
        {
            statusStateMachine.Update();
            return;
        }

        actionStateMachine.Update();

        if (!actionStateMachine.IsBlockingNormalMovement)
        {
            statusStateMachine.Update();
        }

        context.Look.ApplyLook(context.Input.LookInput);

        Vector2 recoil = context.WeaponController
            .WeaponRecoil.Update(Time.deltaTime);

        context.Look.AddRecoil(recoil);

        if (actionStateMachine.IsBlockingNormalMovement)
        {
            return;
        }

        context.Motor.Move(
            context.Input.MoveInput,
            context.Input.IsJumpHeld,
            Time.deltaTime
        );
    }

    /// <summary>
    /// 死亡状態へ遷移する
    /// </summary>
    public void ChangeDeadStatusState()
    {
        actionStateMachine.ChangeNoActionState();
        statusStateMachine.ChangeDeadState();
    }
}
