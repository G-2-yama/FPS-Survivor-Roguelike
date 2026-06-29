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
        gameController.Player.ConsumePendingLevelUp();

        HideUpgradeUI();
        if (gameController.Player.PendingLevelUps > 0)
        {
            gameController.StateMachine.ChangeUpgradeState();
        }
        else
        {
            gameController.StateMachine.ChangePlayingState();
        }
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

        List<UpgradeBase> poolCopy = upgradePool.Where(u => u.IsAvailable()).ToList();

        for (int i = 0; i < 3; i++)
        {
            if (poolCopy.Count == 0)
                break;

            UpgradeBase selected = GetWeightedRandom(poolCopy);

            currentChoices.Add(selected);

            // 重複しないよう削除
            poolCopy.Remove(selected);
        }
    }

    private void SetUpgradePool(Player player)
    {
        foreach(var upgrade in upgradePool)
        {
            upgrade.Initialize(player);
        }
    }

    private UpgradeBase GetWeightedRandom(List<UpgradeBase> pool)
    {
        int totalWeight = 0;

        foreach (var upgrade in pool)
        {
            totalWeight += upgrade.Weight;
        }

        int random = Random.Range(0, totalWeight);

        foreach (var upgrade in pool)
        {
            random -= upgrade.Weight;

            if (random < 0)
                return upgrade;
        }

        return pool[0];
    }
}
