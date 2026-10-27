using Unity.VisualScripting;

public class GameStateMachine : StateMachine<GameState>
{
    private readonly RecoveryUpgradeAutoRecovery autoRecovery;
    private GameState playingState;
    public GameState PlayingState => playingState;

    private EndState endState;
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

        autoRecovery = controller.gameManager.UpgradeManager.GetComponent<RecoveryUpgradeAutoRecovery>();
        autoRecovery?.Initialize(controller, this);

        controller.Player.OnDeath += OnPlayerDeath;

        controller.StartTimer();
        ChangeState(playingState);
    }

    public void ChangePlayingState()
    {
        ChangeState(playingState);
    }

    public void ChangeEndState(GameResult result)
    {
        if (CurrentState == endState)
        {
            return;
        }

        endState.SetResult(result);
        ChangeState(endState);
    }

    public void ChangeUpgradeState()
    {
        if (autoRecovery != null && autoRecovery.TryApplyPendingRecovery())
            return;

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

    private void OnPlayerDeath() => ChangeEndState(GameResult.GameOver);
}
