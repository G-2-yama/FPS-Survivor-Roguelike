/// <summary>
/// スライディングする動作ステート
/// </summary>
public class PlayerSlideActionState : PlayerActionState
{
    public override bool BlocksNormalMovement => true;
    private UnityEngine.Vector3 slideDirection;
    private float originalControllerHeight;
    private UnityEngine.Vector3 originalControllerCenter;

    /// <summary>
    /// スライディング動作ステートを初期化する
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト</param>
    /// <param name="actionStateMachine">動作ステートマシン</param>
    public PlayerSlideActionState(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
        : base(context, actionStateMachine) { }

    /// <summary>接地していてスライドを開始できるか確認する</summary>
    public bool CanEnter()
    {
        return context.Motor.IsGrounded();
    }

    /// <summary>本体寸法を保存し、スライドの方向と姿勢を設定する</summary>
    public override void Enter()
    {
        originalControllerHeight = context.Motor.ControllerHeight;
        originalControllerCenter = context.Motor.ControllerCenter;

        UnityEngine.Vector3 horizontalVelocity = context.Motor.HorizontalVelocity;
        horizontalVelocity.y = 0f;

        if (horizontalVelocity.magnitude >= context.ControlSettings.SlideMinInertiaSpeed)
        {
            slideDirection = horizontalVelocity.normalized;
        }
        else
        {
            slideDirection = context.Player.transform.forward;
            slideDirection.y = 0f;
            slideDirection.Normalize();
        }

        if (slideDirection.sqrMagnitude <= 0f)
        {
            slideDirection = UnityEngine.Vector3.forward;
        }

        context.Motor.SetControllerHeight(originalControllerHeight * context.Motor.SlideHeightMultiplier);
        context.ViewOffset.SetSlideActive(true);
        context.ActionTrail?.Begin(PlayerActionTrail.ActionType.Slide);
        context.ActionConcentrationLine?.Begin(PlayerActionConcentrationLine.ActionType.Slide);
    }

    /// <summary>スライドの継続条件と旋回を処理し、移動を適用する</summary>
    public override void Update()
    {
        if (!context.Controls.CrouchHeld || !context.Motor.IsGrounded())
        {
            actionStateMachine.ChangeToNoActionState();
            return;
        }

        if (context.Commands.TryConsumeJump() && context.TryJump())
        {
            context.Motor.HorizontalVelocity = slideDirection * context.ControlSettings.SlideSpeed;

            if (context.Controls.CrouchHeld)
            {
                context.Commands.ArmSlideOnNextLand();
            }

            actionStateMachine.ChangeToNoActionState();
            return;
        }

        if (context.Locomotion.TryGetMoveDirection(context.Controls.MoveInput, out UnityEngine.Vector3 targetDirection))
        {
            float maxRadiansDelta =
                context.ControlSettings.SlideTurnRateDegreesPerSecond * UnityEngine.Mathf.Deg2Rad * UnityEngine.Time.deltaTime;

            slideDirection = UnityEngine.Vector3.RotateTowards(
                slideDirection,
                targetDirection,
                maxRadiansDelta,
                0f);
        }

        context.Locomotion.MoveSlide(slideDirection, context.ControlSettings.SlideSpeed, UnityEngine.Time.deltaTime);
    }

    /// <summary>通常時の本体寸法とカメラ演出へ戻す</summary>
    public override void Exit()
    {
        context.ViewOffset.SetSlideActive(false);
        context.Motor.SetControllerDimensions(originalControllerHeight, originalControllerCenter);
        slideDirection = UnityEngine.Vector3.zero;
        context.ActionTrail?.End(PlayerActionTrail.ActionType.Slide);
        context.ActionConcentrationLine?.End(PlayerActionConcentrationLine.ActionType.Slide);
    }
}
