using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private readonly GameplayCursorController gameplayCursorController = new GameplayCursorController();

    [SerializeField] private Player player;
    public Player Player => player;

    [SerializeField] private Timer timer;
    public Timer Timer => timer;

    [SerializeField] private GameView gameView;
    public GameView GameView => gameView;

    [SerializeField] private UpgradeManager upgradeManager;
    public UpgradeManager UpgradeManager => upgradeManager;

    [SerializeField] private InventoryController inventoryController;
    public InventoryController InventoryController => inventoryController;

    [SerializeField] private Sounder sounder;
    public Sounder Sounder => sounder;

    private GameController gameController;
    private GameStateMachine gameStateMachine;
    public GameStateMachine GameStateMachine => gameStateMachine;

    public void Start()
    {
        gameController = new GameController(this);
        upgradeManager.Initialize(gameController);
        gameStateMachine = new GameStateMachine(gameController);
        sounder.Play(SoundCategory.BGM);
        gameplayCursorController.UpdateCursor(gameStateMachine.CurrentState);
    }

    private void Update()
    {
        gameStateMachine.Update();
        gameplayCursorController.UpdateCursor(gameStateMachine.CurrentState);
    }
}
