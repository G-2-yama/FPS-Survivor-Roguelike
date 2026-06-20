

public class UpgradeState : GameState
{
    public override CursorActivationMode CursorActivationMode =>
        CursorActivationMode.AlwaysVisible;

    public UpgradeState(GameController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        controller.PauseGame();
        controller.ShowUpgradeUI();
    }

    public override void Update()
    {

    }

    public override void Exit()
    {
        controller.ResumeGame();
        controller.HideUpgradeUI();
    }
}
