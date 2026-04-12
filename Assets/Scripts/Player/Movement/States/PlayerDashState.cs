using UnityEngine;

public class PlayerDashState : PlayerMoveState
{
    private Vector3 dashDirection;
    private float dashTimer;
    private float cooldownTimer;

    /// <summary>
    /// ダッシュ状態を初期化する
    /// </summary>
    /// <param name="controller">プレイヤー制御クラス</param>
    /// <param name="moveStateMachine">移動状態を管理するステートマシン</param>
    public PlayerDashState(PlayerController controller,
                           PlayerMoveStateMachine moveStateMachine)
        : base(controller, moveStateMachine) { }

    /// <summary>
    /// 現在の入力とクールタイムからダッシュ開始可能か判定する
    /// </summary>
    /// <returns>ダッシュ状態へ遷移できる場合はtrue</returns>
    public bool CanEnter()
    {
        return cooldownTimer <= 0f
            && controller.Mover.TryGetMoveDirection(controller.MoveInput, out _);
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
    /// ダッシュ状態へ入ったときに方向と継続時間を初期化する
    /// </summary>
    public override void Enter()
    {
        if (!controller.Mover.TryGetMoveDirection(controller.MoveInput, out dashDirection))
        {
            TransitionToGroundMoveStateByInput();
            return;
        }

        dashTimer = controller.Player.Config.DashDuration;
        controller.Mover.ClearHorizontalVelocity();
    }

    /// <summary>
    /// ダッシュ移動を適用し、継続時間が終わったら次の移動状態へ遷移する
    /// </summary>
    public override void Update()
    {
        float dashStep = Mathf.Min(Time.deltaTime, dashTimer);
        controller.Mover.MoveDash(dashDirection, dashStep);
        dashTimer -= dashStep;

        if (dashTimer > 0f)
        {
            return;
        }

        if (!controller.IsGrounded)
        {
            moveStateMachine.ChangeAirState();
            return;
        }

        TransitionToGroundMoveStateByInput();
    }

    /// <summary>
    /// ダッシュ状態の実行時情報を初期化し、クールタイムを開始する
    /// </summary>
    public override void Exit()
    {
        dashTimer = 0f;
        dashDirection = Vector3.zero;
        cooldownTimer = controller.Player.Config.DashCooldown;
    }
}
