using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private readonly GameplayCursorController gameplayCursorController = new GameplayCursorController();
    private GameplayInputController gameplayInputController;

    [SerializeField] private Player player;
    public Player Player => player;

    [Tooltip("ゲーム状態に応じて操作の有効・無効を切り替えるプレイヤーの入力")]
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private Timer timer;
    public Timer Timer => timer;

    [SerializeField] private GameEndView gameEndView;
    public GameEndView GameEndView => gameEndView;

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
        ApplyStartSettings();
        gameController = new GameController(this);
        gameplayInputController = new GameplayInputController(playerInput);
        upgradeManager.Initialize(gameController);
        gameStateMachine = new GameStateMachine(gameController);
        sounder.Play(SoundCategory.BGM);
        gameplayCursorController.UpdateCursor(gameStateMachine.CurrentState);
        gameplayInputController.UpdateInput(gameStateMachine.CurrentState);
        gameStateMachine.OnStateChanged += gameplayInputController.UpdateInput;
    }

    private void Update()
    {
        gameStateMachine.Update();
        gameplayCursorController.UpdateCursor(gameStateMachine.CurrentState);
    }

    private void ApplyStartSettings()
    {
        if (GameStartSettings.StartingWeapon != null)
            player.Inventory.EquipWeapon(SlotType.LeftMain, GameStartSettings.StartingWeapon);

        player.SetWeaponSync(GameStartSettings.WeaponSync);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugClear()
    {
        timer.ForceFinish();
    }

    public void DebugGameOver()
    {
        player.TakeDamage(player.Health.CurrentHP, 0f);
    }
#endif
}
