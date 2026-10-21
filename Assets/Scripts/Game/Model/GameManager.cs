using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("UI Raycast Routing")]
    [Tooltip("通常プレイ中にUI入力を受け付けるGraphicRaycaster")]
    [SerializeField] private GraphicRaycaster gameplayGraphicRaycaster;

    [Tooltip("レベルアップ中にUI入力を受け付けるGraphicRaycaster")]
    [SerializeField] private GraphicRaycaster upgradeGraphicRaycaster;

    [Tooltip("インベントリ表示中にUI入力を受け付けるGraphicRaycaster")]
    [SerializeField] private GraphicRaycaster inventoryGraphicRaycaster;

    [Tooltip("ゲーム終了画面でUI入力を受け付けるGraphicRaycaster")]
    [SerializeField] private GraphicRaycaster endGraphicRaycaster;

    [SerializeField] private Timer timer;
    public Timer Timer => timer;

    [Tooltip("チュートリアル中に敵の自動出現を止める EnemyGenerator")]
    [SerializeField] private EnemyGenerator enemyGenerator;

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

        if (GameStartSettings.IsTutorial)
        {
            enemyGenerator.DisableAutoSpawn();
            player.DisableExpGain();
            player.MakeInvincible();
        }
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
        ApplyUIRaycastState(gameStateMachine.CurrentState);
        gameStateMachine.OnStateChanged += HandleGameStateChanged;
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

        gameplayCursorController.UpdateCursor(currentState, gamepadOwnsPointer);
    }

    /// <summary>
    /// 状態遷移時に入力対象の画面とレイキャスターを切り替える
    /// </summary>
    /// <param name="currentState">遷移後のゲーム状態</param>
    private void HandleGameStateChanged(GameState currentState)
    {
        gameplayInputController.UpdateInput(currentState);
        ApplyUIRaycastState(currentState);
    }

    /// <summary>
    /// 表示中の画面だけがポインター入力を受けるようレイキャスターを切り替える
    /// </summary>
    /// <param name="currentState">現在のゲーム状態</param>
    private void ApplyUIRaycastState(GameState currentState)
    {
        SetRaycasterEnabled(gameplayGraphicRaycaster, currentState == gameStateMachine.PlayingState);
        SetRaycasterEnabled(upgradeGraphicRaycaster, currentState == gameStateMachine.UpgradeState);
        SetRaycasterEnabled(inventoryGraphicRaycaster, currentState == gameStateMachine.InventoryState);
        SetRaycasterEnabled(endGraphicRaycaster, currentState == gameStateMachine.EndState);
    }

    /// <summary>
    /// レイキャスターの状態が変わるときだけ有効状態を書き換える
    /// </summary>
    /// <param name="raycaster">状態を切り替えるレイキャスター</param>
    /// <param name="enabled">入力を受け付ける場合はtrue</param>
    private static void SetRaycasterEnabled(GraphicRaycaster raycaster, bool enabled)
    {
        if (raycaster != null && raycaster.enabled != enabled)
        {
            raycaster.enabled = enabled;
        }
    }

    /// <summary>
    /// ゲーム開始時に選択されているモードと武器設定を反映する
    /// </summary>
    private void ApplyStartSettings()
    {
        if (GameStartSettings.Mode != null)
            timer.SetTimeLimit(GameStartSettings.Mode.TimeLimitSeconds);

        if (GameStartSettings.IsTutorial)
            timer.SetTimeLimit(-1f);

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
