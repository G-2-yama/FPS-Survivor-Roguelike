using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Player player;
    public Player Player => player;
    
    [SerializeField] private Timer timer;
    public Timer Timer => timer;

    [SerializeField] private GameView gameView;
    public GameView GameView => gameView;

    [SerializeField] private UpgradeManager upgradeManager;
    public UpgradeManager UpgradeManager => upgradeManager;

    private GameController gameController;
    private GameStateMachine gameStateMachine;
    public GameStateMachine GameStateMachine => gameStateMachine;

    private void Start()
    {
        gameController = new GameController(this);
        gameStateMachine = new GameStateMachine(gameController);
    }

    private void Update()
    {
        gameStateMachine.Update();
    }
}