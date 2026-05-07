using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private UpgradeView upgradeView;

    [SerializeField] private List<UpgradeBase> upgradePool = new List<UpgradeBase>();
    private List<UpgradeBase> currentChoices = new List<UpgradeBase>();

    private GameController gameController;

    public void Initialize(GameController controller)
    {
        this.gameController = controller;
        SetUpgradePool(controller.Player);
    }
    /// <summary>
    /// アップグレードの選択肢がクリックされたときの処理
    /// </summary>
    /// <param name="upgradeIndex">選択されたアップグレードのインデックス</param>
    public void OnUpgradeButtonClicked(int upgradeIndex)
    {
        var selectedUpgrade = currentChoices[upgradeIndex];
        selectedUpgrade.Apply();
        gameController.StateMachine.ChangePlayingState();
        // アップグレードUIを閉じる
        HideUpgradeUI();
    }

    /// <summary>
    /// アップグレードUIを表示
    /// </summary>
    public void ShowUpgradeUI()
    {
        GenerateChoices();
        upgradeView.Setup(currentChoices);
        upgradeView.Show();
    }

    /// <summary>
    /// アップグレードUIを非表示
    /// </summary>
    public void HideUpgradeUI()
    {
        upgradeView.Hide();
    }

    /// <summary>
    /// ランダムで強化項目を3つ選ぶ
    /// </summary>
    private void GenerateChoices()
    {
        currentChoices.Clear();

        // 利用可能なアップグレードのプールからランダムに3つ選ぶ
        List<UpgradeBase> poolCopy = upgradePool
            .Where(u => u.IsAvailable()).ToList();

        for (int i = 0; i < 3; i++)
        {
            if (poolCopy.Count == 0) break;

            int index = Random.Range(0, poolCopy.Count);
            currentChoices.Add(poolCopy[index]);
            poolCopy.RemoveAt(index);
        }
    }

    private void SetUpgradePool(Player player)
    {
        foreach(var upgrade in upgradePool)
        {
            upgrade.Initialize(player);
        }
    }
}
