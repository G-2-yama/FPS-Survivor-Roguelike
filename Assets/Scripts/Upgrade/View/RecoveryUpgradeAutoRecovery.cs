using UnityEngine;
using UnityEngine.UI;

/// <summary>回復候補だけになった後の表示省略と自動回復を、今回のプレイ中だけ管理する。</summary>
public class RecoveryUpgradeAutoRecovery : MonoBehaviour
{
    [SerializeField] private RecoveryUpgrade recoveryUpgrade;
    [SerializeField] private Button promptButton;

    private GameController controller;
    private GameStateMachine stateMachine;
    private bool autoRecoveryEnabled;

    public void Initialize(GameController gameController, GameStateMachine gameStateMachine)
    {
        if (stateMachine != null)
            stateMachine.OnStateChanged -= HandleStateChanged;

        controller = gameController;
        stateMachine = gameStateMachine;
        autoRecoveryEnabled = false;
        if (promptButton != null)
        {
            promptButton.onClick.RemoveListener(EnableAutoRecovery);
            promptButton.onClick.AddListener(EnableAutoRecovery);
        }
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
