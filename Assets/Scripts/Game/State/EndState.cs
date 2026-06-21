

public class EndState : GameState
{
    public override CursorActivationMode CursorActivationMode =>
        CursorActivationMode.AlwaysVisible;

    public EndState(GameController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        controller.ShowGameEnd();
    }

    public override void Update()
    {
        // ゲーム終了中の更新処理
    }

    public override void Exit()
    {
        // ゲーム終了後の後処理
    }
}
