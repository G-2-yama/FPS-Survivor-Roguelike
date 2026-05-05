using Unity.VisualScripting;

public class GameStateMachine : StateMachine<GameState>
{
    private GameState playingState;
    public GameState PlayingState => playingState;

    private GameState endState;
    public GameState EndState => endState;
    
    private GameState upgradeState;
    public GameState UpgradeState => upgradeState;

    public GameStateMachine(GameController controller)
    {
        playingState = new PlayingState(controller);
        endState = new EndState(controller);
        upgradeState = new UpgradeState(controller);

        controller.StartTimer();
        ChangeState(playingState);
    }

    public void ChangePlayingState()
    {
        ChangeState(playingState);
    }

    public void ChangeEndState()
    {
        ChangeState(endState);
    }

    public void ChangeUpgradeState()
    {
        ChangeState(upgradeState);
    }

}