using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ゲーム全体の開始処理とゲーム状態ごとの入力制御を管理する
/// </summary>
public class GameManager : MonoBehaviour
{
    private readonly GameplayCursorController gameplayCursorController = new GameplayCursorController();
    private GameplayInputController gameplayInputController;

    [SerializeField] private Player player;
    public Player Player => player;

    [Tooltip("ゲーム状態に応じてプレイヤー入力を切り替える PlayerInput")]
    [SerializeField] private PlayerInput playerInput;

    [Tooltip("ゲーム内カーソルの入力元を切り替えるコンポーネント")]
    [SerializeField] private GamepadVirtualMouseCursor gamepadVirtualMouseCursor;

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

    private GameStats gameStats;
    public GameStats GameStats => gameStats;

    /// <summary>
    /// ゲーム中に集計する統計情報を初期化する
    /// </summary>
    private void Awake()
    {
        gameStats = new GameStats();
    }

    /// <summary>
    /// ゲーム開始時の設定と各管理クラスを初期化する
    /// </summary>
    public void Start()
    {
        ApplyStartSettings();
        gameController = new GameController(this);
        gameplayInputController = new GameplayInputController(playerInput);
        upgradeManager.Initialize(gameController);
        gameStateMachine = new GameStateMachine(gameController);
        sounder.Play(SoundCategory.BGM);
        UpdateCursorControl();
        gameplayInputController.UpdateInput(gameStateMachine.CurrentState);
        gameStateMachine.OnStateChanged += gameplayInputController.UpdateInput;
    }

    /// <summary>
    /// ゲーム状態と直近の入力元に応じてカーソルを更新する
    /// </summary>
    private void Update()
    {
        gameStateMachine.Update();
        UpdateCursorControl();
    }

    /// <summary>
    /// カーソルを使えるゲーム状態か判定し、表示と入力元へ反映する
    /// </summary>
    private void UpdateCursorControl()
    {
        GameState currentState = gameStateMachine.CurrentState;
        bool cursorAllowed = gameplayCursorController.IsCursorAllowed(currentState);
        bool gamepadOwnsPointer = gamepadVirtualMouseCursor != null
            && gamepadVirtualMouseCursor.UpdatePointer(cursorAllowed);

        if (currentState == gameStateMachine.UpgradeState && gamepadOwnsPointer)
        {
            upgradeManager.FocusUpgradeAtScreenPosition(gamepadVirtualMouseCursor.CursorPosition);
        }

        gameplayCursorController.UpdateCursor(currentState, gamepadOwnsPointer);
    }

    /// <summary>
    /// ゲーム開始時に選択されているモードと武器設定を反映する
    /// </summary>
    private void ApplyStartSettings()
    {
        if (GameStartSettings.Mode != null)
            timer.SetTimeLimit(GameStartSettings.Mode.TimeLimitSeconds);

        if (GameStartSettings.StartingWeapon != null)
            player.Inventory.EquipWeapon(SlotType.LeftMain, GameStartSettings.StartingWeapon);

        player.SetWeaponSync(GameStartSettings.WeaponSync);
    }

    /// <summary>
    /// ゲーム終了時に統計情報を破棄する
    /// </summary>
    private void OnDestroy()
    {
        gameStats?.Dispose();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// デバッグ操作でゲームを終了させる
    /// </summary>
    public void DebugClear()
    {
        timer.ForceFinish();
    }

    /// <summary>
    /// デバッグ操作でプレイヤーのHPをゼロにする
    /// </summary>
    public void DebugGameOver()
    {
        player.TakeDamage(player.Health.CurrentHP, 0f);
    }
#endif
}
