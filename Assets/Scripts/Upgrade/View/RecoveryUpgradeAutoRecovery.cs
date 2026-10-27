using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>回復候補だけになった後の表示省略と自動回復を、今回のプレイ中だけ管理する。</summary>
public class RecoveryUpgradeAutoRecovery : MonoBehaviour
{
    [SerializeField] private RecoveryUpgrade recoveryUpgrade;
    [SerializeField] private RectTransform upgradeCanvas;
    [SerializeField] private TMP_FontAsset promptFont;
    [SerializeField, TextArea] private string promptText =
        "今後アップグレード画面を表示しない\n（レベルアップ時に自動回復します）";
    [SerializeField] private Vector2 promptPosition = new Vector2(0f, 85f);
    [SerializeField] private Vector2 promptSize = new Vector2(1450f, 96f);
    [SerializeField, Min(1f)] private float promptFontSize = 26f;

    private GameController controller;
    private GameStateMachine stateMachine;
    private Button promptButton;
    private bool autoRecoveryEnabled;

    public void Initialize(GameController gameController, GameStateMachine gameStateMachine)
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged -= HandleStateChanged;

        controller = gameController;
        stateMachine = gameStateMachine;
        autoRecoveryEnabled = false;
        CreatePrompt();
        SetPromptVisible(false);
        stateMachine.OnStateChanged += HandleStateChanged;
    }

    /// <summary>通常の候補が残っていなければ、保留レベルをすべて回復として消費する。</summary>
    public bool TryApplyPendingRecovery()
    {
        if (!autoRecoveryEnabled || !IsRecoveryAvailable())
            return false;

        while (controller.Player.PendingLevelUps > 0)
        {
            recoveryUpgrade.Apply();
            controller.Player.ConsumePendingLevelUp();
        }

        return true;
    }

    private bool IsRecoveryAvailable()
    {
        if (controller == null || recoveryUpgrade == null)
            return false;

        recoveryUpgrade.Initialize(controller.Player);
        return recoveryUpgrade.IsAvailable();
    }

    private void HandleStateChanged(GameState state)
    {
        SetPromptVisible(state == stateMachine.UpgradeState
            && !autoRecoveryEnabled && IsRecoveryAvailable());
    }

    private void EnableAutoRecovery()
    {
        if (stateMachine == null || stateMachine.CurrentState != stateMachine.UpgradeState
            || controller.Player.PendingLevelUps <= 0 || !IsRecoveryAvailable())
            return;

        autoRecoveryEnabled = true;
        SetPromptVisible(false);
        TryApplyPendingRecovery();
        stateMachine.ChangePlayingState();
    }

    private void CreatePrompt()
    {
        if (promptButton != null || upgradeCanvas == null)
            return;

        var prompt = new GameObject("AutoRecoveryPrompt", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(Button));
        prompt.layer = upgradeCanvas.gameObject.layer;
        var rect = prompt.GetComponent<RectTransform>();
        rect.SetParent(upgradeCanvas, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = promptPosition;
        rect.sizeDelta = promptSize;

        var label = prompt.GetComponent<TextMeshProUGUI>();
        if (promptFont != null)
            label.font = promptFont;
        label.text = promptText;
        label.fontSize = promptFontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.fontStyle = FontStyles.Underline;
        label.color = Color.white;
        label.raycastTarget = true;

        promptButton = prompt.GetComponent<Button>();
        promptButton.targetGraphic = label;
        promptButton.onClick.AddListener(EnableAutoRecovery);
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptButton != null)
            promptButton.gameObject.SetActive(visible);
    }

    private void OnDestroy()
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged -= HandleStateChanged;
        if (promptButton != null)
            promptButton.onClick.RemoveListener(EnableAutoRecovery);
    }
}
