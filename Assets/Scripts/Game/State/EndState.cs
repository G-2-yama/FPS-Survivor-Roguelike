public class EndState : GameState
{
    public override CursorActivationMode CursorActivationMode =>
        CursorActivationMode.AlwaysVisible;

    public override bool AcceptsPlayerInput => false;

    private GameResult result;

    public EndState(GameController controller) : base(controller)
    {
    }

    public void SetResult(GameResult result)
    {
        this.result = result;
    }

    public override void Enter()
    {
        controller.StopTimer();
        controller.PauseGame();
        controller.ShowGameEnd(result);
    }
}
