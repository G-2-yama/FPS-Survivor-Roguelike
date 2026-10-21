public class TutorialMenuState : GameState
{
    public override CursorActivationMode CursorActivationMode =>
        CursorActivationMode.AlwaysVisible;

    public override bool AcceptsPlayerInput => false;

    public TutorialMenuState(GameController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        controller.PauseGame();
    }

    public override void Exit()
    {
        controller.ResumeGame();
    }
}
