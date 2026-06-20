using Unity.VisualScripting;

public class GameStateMachine : StateMachine<GameState>
{
    private GameState playingState;
    public GameState PlayingState => playingState;

    private GameState endState;
    public GameState EndState => endState;
    
    private GameState upgradeState;
    public GameState UpgradeState => upgradeState;

    private GameState inventoryState;
    public GameState InventoryState => inventoryState;

    public GameStateMachine(GameController controller)
    {
        playingState = new PlayingState(controller);
        endState = new EndState(controller);
        upgradeState = new UpgradeState(controller);
        inventoryState = new InventoryState(controller);

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

    public void ChangeInventoryState()
    {
        ChangeState(inventoryState);
    }

    public void ToggleInventoryState()
    {
        if (CurrentState == playingState)
        {
            ChangeState(inventoryState);
            return;
        }

        if (CurrentState == inventoryState)
        {
            ChangeState(playingState);
        }
    }

}
