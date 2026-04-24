using UnityEngine;

/// <summary>
/// プレイヤーがダッシュしている動作ステート
/// </summary>
public class PlayerDashActionState : PlayerActionState
{
    /// <summary>
    /// ダッシュ中は通常移動を止め、ダッシュ専用の移動量だけを適用する
    /// </summary>
    public override bool BlocksNormalMovement => true;

    /// <summary>
    /// ダッシュ開始時に確定した移動方向
    /// </summary>
    private Vector3 dashDirection;

    /// <summary>
    /// ダッシュ動作の残り時間
    /// </summary>
    private float dashTimer;

    /// <summary>
    /// 次のダッシュ開始までの残り待ち時間
    /// </summary>
    private float cooldownTimer;

    /// <summary>
    /// ダッシュ動作を初期化する
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト</param>
    /// <param name="actionStateMachine">動作状態を管理するステートマシン</param>
    public PlayerDashActionState(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
        : base(context, actionStateMachine) { }

    /// <summary>
    /// 現在の入力とクールタイムからダッシュ開始可能か判定する
    /// </summary>
    /// <returns>ダッシュ動作へ遷移できる場合はtrue</returns>
    public bool CanEnter()
    {
        return cooldownTimer <= 0f
            && context.Locomotion.TryGetMoveDirection(context.Controls.MoveInput, out _);
    }

    /// <summary>
    /// ダッシュのクールタイムを更新する
    /// </summary>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void UpdateCooldown(float deltaTime)
    {
        if (cooldownTimer <= 0f)
        {
            return;
        }

        cooldownTimer = Mathf.Max(0f, cooldownTimer - deltaTime);
    }

    /// <summary>
    /// ダッシュ動作へ入ったときに方向と継続時間を初期化する
    /// </summary>
    public override void Enter()
    {
        if (!context.Locomotion.TryGetMoveDirection(context.Controls.MoveInput, out dashDirection))
        {
            actionStateMachine.ChangeToNoActionState();
            return;
        }

        dashTimer = context.Config.DashDuration;
        context.Locomotion.ClearHorizontalVelocity();
    }

    /// <summary>
    /// ダッシュ移動を適用し、継続時間が終わったら通常動作へ遷移する
    /// </summary>
    public override void Update()
    {
        float dashStep = Mathf.Min(Time.deltaTime, dashTimer);
        context.Locomotion.MoveDash(dashDirection, dashStep);
        dashTimer -= dashStep;

        if (dashTimer > 0f)
        {
            return;
        }

        actionStateMachine.ChangeToNoActionState();
    }

    /// <summary>
    /// ダッシュ動作の実行時情報を初期化し、クールタイムを開始する
    /// </summary>
    public override void Exit()
    {
        dashTimer = 0f;
        dashDirection = Vector3.zero;
        cooldownTimer = context.Config.DashCooldown;
    }
}
