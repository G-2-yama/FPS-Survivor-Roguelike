using UnityEngine;

/// <summary>
/// 武器ごとの差分パラメータを保持するデータ定義
/// 攻撃システム側はこのデータを受け取り、共通処理で攻撃を実行する
/// </summary>
[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public virtual bool IsEmpty => false;

    [Header("Identity")]
    [SerializeField] private WeaponIdentity identity;
    public WeaponIdentity Identity => identity;

    [Header("Visual")]
    [SerializeField] private WeaponVisual visual;
    public WeaponVisual Visual => visual;

    [Header("Classification")]
    [SerializeField] private WeaponClassification classification;
    public WeaponClassification Classification => classification;

    [Header("Trigger")]
    [SerializeField] private WeaponTriggerConfig trigger;
    public WeaponTriggerConfig Trigger => trigger;

    [Header("Stats")]
    [SerializeField] private WeaponLevelTable levelTable;

    public int MaxLevel => levelTable.MaxLevel;
    public WeaponStats GetStats(int level) => levelTable.GetStats(level);
    public bool AutoFire => trigger.AutoFire;
    public bool AutoReload => trigger.AutoReload;
    public WeaponTriggerType TriggerType => trigger.TriggerType;
    public WeaponType WeaponType => classification.WeaponType;
    public FireModeData FireModeData => classification.FireModeData;
    public GameObject WeaponModelPrefab => visual.WeaponModelPrefab;
    public Sprite Icon => identity.Icon;
    public string DisplayName => identity.DisplayName;
    public string WeaponId => identity.WeaponId;
}

/// <summary>
/// レベルとWeaponStatsの対応を管理する
/// 「どのレベルでどのStatsか」という目的だけを持つ
/// </summary>
[System.Serializable]
public sealed class WeaponLevelTable
{
    [SerializeField] private WeaponStats baseStats;
    [SerializeField] private WeaponStats[] levelStats;

    /// <summary>最大レベル（0=baseのみの場合は0）</summary>
    public int MaxLevel => levelStats != null ? levelStats.Length : 0;

    /// <summary>
    /// 指定レベルのStatsを返す
    /// 不変オブジェクトなので参照を直接返しても安全
    /// </summary>
    public WeaponStats GetStats(int level)
    {
        if (level <= 0 || levelStats == null || levelStats.Length == 0)
            return baseStats;

        int index = Mathf.Clamp(level - 1, 0, levelStats.Length - 1);
        return levelStats[index];
    }
}

/// <summary>
/// 武器の識別・UI表示に関するデータ
/// 「この武器が何者か」という目的
/// </summary>
[System.Serializable]
public class WeaponIdentity
{
    [SerializeField] private string weaponId = "weapon_default";
    [SerializeField] private string displayName = "New Weapon";
    [SerializeField] private Sprite icon;

    public string WeaponId => weaponId;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
}

/// <summary>
/// 武器の見た目・演出に関するデータ
/// 「プレイヤーの目に何が見えるか」という目的
/// </summary>
[System.Serializable]
public class WeaponVisual
{
    [SerializeField] private GameObject weaponModelPrefab;
    public GameObject WeaponModelPrefab => weaponModelPrefab;
}

/// <summary>
/// 射撃トリガーの挙動に関するデータ
/// 「ボタン入力がどう射撃に変換されるか」という目的
/// </summary>
[System.Serializable]
public class WeaponTriggerConfig
{
    [SerializeField] private bool autoFire = false;
    [SerializeField] private bool autoReload = false;
    [SerializeField] private WeaponTriggerType triggerType = WeaponTriggerType.FullAuto;

    public bool AutoFire => autoFire;
    public bool AutoReload => autoReload;
    public WeaponTriggerType TriggerType => triggerType;
}

/// <summary>
/// ゲームプレイ上の分類に関するデータ
/// 「武器がゲームシステム内でどの枠に属するか」という目的
/// </summary>
[System.Serializable]
public class WeaponClassification
{
    [SerializeField] private WeaponType weaponType = WeaponType.Main;
    [SerializeField] private FireModeData fireModeData;

    public WeaponType WeaponType => weaponType;
    public FireModeData FireModeData => fireModeData;
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