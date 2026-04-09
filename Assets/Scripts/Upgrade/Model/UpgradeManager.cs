using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    [SerializeField] private UpgradeView upgradeView;

    private List<UpgradeBase> upgradePool = new List<UpgradeBase>();
    private List<UpgradeBase> currentChoices = new List<UpgradeBase>();

    void Start()
    {
        UpgradeBase levelUpUpgrade = new LevelUp();
        upgradePool.Add(levelUpUpgrade);
    }

    /// <summary>
    /// アップグレードの選択肢がクリックされたときの処理
    /// </summary>
    /// <param name="upgradeIndex">選択されたアップグレードのインデックス</param>
    public void OnUpgradeButtonClicked(int upgradeIndex)
    {
        var selectedUpgrade = currentChoices[upgradeIndex];
        selectedUpgrade.Apply(weapon.gameObject);

        // アップグレードUIを閉じる
        HideUpgradeUI();
    }

    /// <summary>
    /// アップグレードUIを表示し、ゲームを一時停止する
    /// </summary>
    public void ShowUpgradeUI()
    {
        Time.timeScale = 0f;
        GenerateChoices();
        upgradeView.Setup(currentChoices);
        upgradeView.Show();
    }

    /// <summary>
    /// アップグレードUIを非表示にし、ゲームを再開する
    /// </summary>
    public void HideUpgradeUI()
    {
        Time.timeScale = 1f;
        upgradeView.Hide();
    }

    /// <summary>
    /// ランダムで強化項目を3つ選ぶ
    /// </summary>
    private void GenerateChoices()
    {
        currentChoices.Clear();

        List<UpgradeBase> poolCopy = new List<UpgradeBase>(upgradePool);

        for (int i = 0; i < 3; i++)
        {
            if (poolCopy.Count == 0) break;

            int index = Random.Range(0, poolCopy.Count);
            currentChoices.Add(poolCopy[index]);
            poolCopy.RemoveAt(index);
        }
    }
}
