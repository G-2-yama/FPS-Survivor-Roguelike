public class InventoryState : GameState
{
    public override CursorActivationMode CursorActivationMode =>
        CursorActivationMode.AlwaysVisible;

    public InventoryState(GameController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        controller.PauseGame();
        controller.ShowInventoryUI();
    }

    public override void Exit()
    {
        controller.ResumeGame();
        controller.HideInventoryUI();
    }
}
