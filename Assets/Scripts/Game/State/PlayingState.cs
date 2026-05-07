

public class PlayingState : GameState
{
    public PlayingState(GameController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        controller.StartTimer();
        controller.ResumeGame();
        controller.Player.OnLevelUp += OnLevelUp;
        controller.Timer.OnTimerFinished += OnTimerFinished;
    }

    public override void Update()
    {
        // プレイ中の更新処理
    }

    public override void Exit()
    {
        controller.StopTimer();
        controller.Player.OnLevelUp -= OnLevelUp;
        controller.Timer.OnTimerFinished -= OnTimerFinished;
    }

    private void OnLevelUp()    => controller.StateMachine.ChangeUpgradeState();
    private void OnTimerFinished() => controller.StateMachine.ChangeEndState();
}