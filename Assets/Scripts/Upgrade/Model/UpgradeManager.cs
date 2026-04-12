using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private UpgradeView upgradeView;

    [SerializeField] private List<WeaponData> weaponDatas;

    private List<UpgradeBase> upgradePool = new List<UpgradeBase>();
    private List<UpgradeBase> currentChoices = new List<UpgradeBase>();

    void Start()
    {
        SetUpgradePool();
    }

    /// <summary>
    /// アップグレードの選択肢がクリックされたときの処理
    /// </summary>
    /// <param name="upgradeIndex">選択されたアップグレードのインデックス</param>
    public void OnUpgradeButtonClicked(int upgradeIndex)
    {
        var selectedUpgrade = currentChoices[upgradeIndex];
        selectedUpgrade.Apply();

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

    private void SetUpgradePool()
    {   
        UpgradeBase LeftWeaponLevelUpUpgrade = new LevelUp("左武器レベルアップ", "武器のレベルを上げます", player, false);
        upgradePool.Add(LeftWeaponLevelUpUpgrade);
        UpgradeBase RightWeaponLevelUpUpgrade = new LevelUp("右武器レベルアップ", "武器のレベルを上げます", player, true);
        upgradePool.Add(RightWeaponLevelUpUpgrade);
        UpgradeBase ShotgunUnlockUpgrade = new WeaponUnlock("ショットガンアンロック", "ショットガンをアンロックします", weaponDatas[1], player, true);
        upgradePool.Add(ShotgunUnlockUpgrade);
        UpgradeBase HandgunUnlockUpgrade = new WeaponUnlock("ハンドガンアンロック", "ハンドガンをアンロックします", weaponDatas[0], player, false);
        upgradePool.Add(HandgunUnlockUpgrade);
    }
}
