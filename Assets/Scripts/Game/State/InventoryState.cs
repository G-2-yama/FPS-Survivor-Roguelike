public class InventoryState : GameState
{
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
