/// <summary>ポーズメニュー中の時間とプレイヤー入力を停止する</summary>
public sealed class PauseMenuState : GameState
{
    public override CursorActivationMode CursorActivationMode => CursorActivationMode.AlwaysVisible;
    public override bool AcceptsPlayerInput => false;

    public PauseMenuState(GameController controller) : base(controller) { }

    public override void Enter() => controller.PauseGame();
    public override void Exit() => controller.ResumeGame();
}
