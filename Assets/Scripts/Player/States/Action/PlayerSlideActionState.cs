/// <summary>
/// スライディングする動作ステート。
/// </summary>
public class PlayerSlideActionState : PlayerActionState
{
    public override bool BlocksNormalMovement => true;
    private UnityEngine.Vector3 slideDirection;
    private float originalControllerHeight;
    private UnityEngine.Vector3 originalControllerCenter;

    /// <summary>
    /// スライディング動作ステートを初期化する。
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト。</param>
    /// <param name="actionStateMachine">動作ステートマシン。</param>
    public PlayerSlideActionState(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
        : base(context, actionStateMachine) { }

    public bool CanEnter()
    {
        return context.Motor.IsGrounded();
    }

    public override void Enter()
    {
        originalControllerHeight = context.Motor.ControllerHeight;
        originalControllerCenter = context.Motor.ControllerCenter;

        UnityEngine.Vector3 horizontalVelocity = context.Motor.HorizontalVelocity;
        horizontalVelocity.y = 0f;

        if (horizontalVelocity.magnitude >= context.Config.SlideMinInertiaSpeed)
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

        context.Motor.SetControllerHeight(originalControllerHeight * 0.5f);
        context.ViewOffset.SetSlideActive(true);
    }

    public override void Update()
    {
        if (!context.Controls.CrouchHeld || !context.Motor.IsGrounded())
        {
            actionStateMachine.ChangeToNoActionState();
            return;
        }

        if (context.Commands.TryConsumeJump() && context.TryJump())
        {
            context.Motor.HorizontalVelocity = slideDirection * context.Config.SlideSpeed;

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
                context.Config.SlideTurnRateDegreesPerSecond * UnityEngine.Mathf.Deg2Rad * UnityEngine.Time.deltaTime;

            slideDirection = UnityEngine.Vector3.RotateTowards(
                slideDirection,
                targetDirection,
                maxRadiansDelta,
                0f);
        }

        context.Locomotion.MoveSlide(slideDirection, context.Config.SlideSpeed, UnityEngine.Time.deltaTime);
    }

    public override void Exit()
    {
        context.ViewOffset.SetSlideActive(false);
        context.Motor.SetControllerDimensions(originalControllerHeight, originalControllerCenter);
        slideDirection = UnityEngine.Vector3.zero;
    }
}
