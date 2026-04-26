using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private UpgradeView upgradeView;

    [SerializeField] private List<WeaponData> weaponDatas;
    [SerializeField] private List<WeaponData> abilityDatas;
    [SerializeField] private List<WeaponData> autoWeaponDatas;

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
        UpgradeBase LeftWeaponLevelUpUpgrade = new LevelUp("左武器レベルアップ", "武器のレベルを上げます",WeaponType.Main ,player, false);
        upgradePool.Add(LeftWeaponLevelUpUpgrade);
        UpgradeBase RightWeaponLevelUpUpgrade = new LevelUp("右武器レベルアップ", "武器のレベルを上げます",WeaponType.Main ,player, true);
        upgradePool.Add(RightWeaponLevelUpUpgrade);
        UpgradeBase LeftAbilityLevelUpUpgrade = new LevelUp("左アビリティレベルアップ", "アビリティのレベルを上げます", WeaponType.Ability, player, false);
        upgradePool.Add(LeftAbilityLevelUpUpgrade);
        UpgradeBase RightAbilityLevelUpUpgrade = new LevelUp("右アビリティレベルアップ", "アビリティのレベルを上げます", WeaponType.Ability, player, true);
        upgradePool.Add(RightAbilityLevelUpUpgrade);
        UpgradeBase LeftAutoWeaponLevelUpUpgrade = new LevelUp("左オート武器レベルアップ", "オート武器のレベルを上げます", WeaponType.AutoWeapon, player, false);
        upgradePool.Add(LeftAutoWeaponLevelUpUpgrade);
        UpgradeBase RightAutoWeaponLevelUpUpgrade = new LevelUp("右オート武器レベルアップ", "オート武器のレベルを上げます", WeaponType.AutoWeapon, player, true);
        upgradePool.Add(RightAutoWeaponLevelUpUpgrade);


        UpgradeBase ShotgunUnlockUpgrade = new Unlock("ショットガンアンロック", "ショットガンをアンロックします", weaponDatas[1], player, true);
        upgradePool.Add(ShotgunUnlockUpgrade);
        UpgradeBase HandgunUnlockUpgrade = new Unlock("ハンドガンアンロック", "ハンドガンをアンロックします", weaponDatas[0], player, false);
        upgradePool.Add(HandgunUnlockUpgrade);
        UpgradeBase RifleUnlockUpgrade = new Unlock("ライフルアンロック", "ライフルをアンロックします", weaponDatas[2], player, true);
        upgradePool.Add(RifleUnlockUpgrade);
        
        UpgradeBase GrenadeUnlockUpgrade = new Unlock("グレネードアンロック", "グレネードをアンロックします", abilityDatas[0], player, false);
        upgradePool.Add(GrenadeUnlockUpgrade);
        UpgradeBase SuperShotUnlockUpgrade = new Unlock("スーパーショットアンロック", "スーパースキルをアンロックします", abilityDatas[1], player, true);
        upgradePool.Add(SuperShotUnlockUpgrade);

        UpgradeBase LeftAutoWeaponUnlockUpgrade = new Unlock("左オート武器アンロック", "左手のオート武器をアンロックします", autoWeaponDatas[0], player, false);
        upgradePool.Add(LeftAutoWeaponUnlockUpgrade);
        UpgradeBase RightAutoWeaponUnlockUpgrade = new Unlock("右オート武器アンロック", "右手のオート武器をアンロックします", autoWeaponDatas[0], player, true);
        upgradePool.Add(RightAutoWeaponUnlockUpgrade);
    }
}
