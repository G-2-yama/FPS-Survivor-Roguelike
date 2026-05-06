using UnityEngine;

/// <summary>
/// 武器ごとの差分パラメータを保持するデータ定義
/// 攻撃システム側はこのデータを受け取り、共通処理で攻撃を実行する
/// </summary>
[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string weaponId = "weapon_default";
    public string WeaponId => weaponId;

    [SerializeField] private string displayName = "New Weapon";
    public string DisplayName => displayName;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Header("Visual")]
    [SerializeField] private GameObject weaponModelPrefab;
    public GameObject WeaponModelPrefab => weaponModelPrefab;

    [Header("Type")]
    [SerializeField] private FireModeData fireModeData;
    public FireModeData FireModeData => fireModeData;

    [SerializeField] private WeaponType weaponType = WeaponType.Main;
    public WeaponType WeaponType => weaponType;

    [Header("Trigger")]
    [SerializeField] private bool autoFire = false;
    public bool AutoFire => autoFire;

    [SerializeField] private bool autoReload = false;
    public bool AutoReload => autoReload;

    [SerializeField] private WeaponTriggerType triggerType = WeaponTriggerType.FullAuto;
    public WeaponTriggerType TriggerType => triggerType;

    [Header("Stats")]
    [SerializeField] private WeaponStats baseStats;
    public WeaponStats BaseStats => baseStats;

    [SerializeField] private WeaponStats[] levelStats;

    /// <summary>
    /// 指定レベルのステータスを返す
    /// level=0ならbaseStats、それ以降はlevelStats[level-1]のスナップショット
    /// </summary>
    public WeaponStats CreateStats(int level)
    {
        if (level <= 0 || levelStats == null || levelStats.Length == 0)
            return baseStats.Clone();

        int index = Mathf.Clamp(level - 1, 0, levelStats.Length - 1);
        return levelStats[index].Clone();
    }

    /// <summary>
    /// 最大レベルを返す（level=0がベースなので+1）
    /// </summary>
    public int MaxLevel => levelStats != null ? levelStats.Length : 0;
}

public enum WeaponTriggerType
{
    SemiAuto = 0,
    FullAuto = 1,
}

public enum WeaponType
{
    Main = 0,
    Ability = 1,
    AutoWeapon = 2,
}
