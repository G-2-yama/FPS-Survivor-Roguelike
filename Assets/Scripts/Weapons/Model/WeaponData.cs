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

    [Header("Fire Mode")]
    [SerializeField] private FireModeData fireModeData;
    public FireModeData FireModeData => fireModeData;
    

    [Header("Attack")]

    [SerializeField] private WeaponTriggerType triggerType = WeaponTriggerType.FullAuto;
    public WeaponTriggerType TriggerType => triggerType;

    [SerializeField, Min(0f)] private int damage = 10;
    public float Damage => damage;

    [SerializeField, Min(0.01f)] private float fireInterval = 0.12f;
    public float FireInterval => fireInterval;

    [SerializeField, Range(0f, 45f)] private float spreadAngle = 0.5f;
    public float SpreadAngle => spreadAngle;


    [Header("Recoil")]
    [SerializeField] private float recoilX = 1.2f;
    public float RecoilX => recoilX;

    [SerializeField] private float recoilY = 0.4f;
    public float RecoilY => recoilY;

    [SerializeField] private float recoilRecoverySpeed = 8f;
    public float RecoilRecoverySpeed => recoilRecoverySpeed;


    [Header("ADS")]
    [SerializeField] private float adsZoom = 60f;
    public float AdsZoom => adsZoom;

    [SerializeField] private float adsSpreadMultiplier = 0.4f;
    public float AdsSpreadMultiplier => adsSpreadMultiplier;

    [SerializeField] private float adsRecoilMultiplier = 0.6f;
    public float AdsRecoilMultiplier => adsRecoilMultiplier;


    [Header("Burst")]

    [SerializeField, Min(1)] private int burstCount = 1;
    public int BurstCount => burstCount;

    [SerializeField, Min(0f)] private float burstInterval = 0.05f;
    public float BurstInterval => burstInterval;


    [Header("Ammo")]
    [SerializeField] private bool useMagazine = true;
    public bool UseMagazine => useMagazine;

    [SerializeField, Min(1)] private int magazineSize = 30;
    public int MagazineSize => magazineSize;

    [SerializeField, Min(0f)] private float reloadTime = 1.6f;
    public float ReloadTime => reloadTime;

    [SerializeField, Min(0)] private int maxReserveAmmo = 120;
    public int MaxReserveAmmo => maxReserveAmmo;


    [Header("Level")]
    [SerializeField] private WeaponStats[] levelBonusData;
    public WeaponStats[] LevelBonusData => levelBonusData;

    public WeaponStats CreateBonusStats(int level)
    {
        var stats = new WeaponStats
        {
            Damage = damage,
            FireInterval = fireInterval,
            SpreadAngle = spreadAngle,
            RecoilX = recoilX,
            RecoilY = recoilY,
            MagazineSize = magazineSize,
            ReloadTime = reloadTime,
            RecoilRecoverySpeed = recoilRecoverySpeed,
            BurstCount = burstCount,
            BurstInterval = burstInterval
        };

        if (level < levelBonusData.Length)
        {
            stats.Add(levelBonusData[level]);
        }

        return stats;
    }
}

public enum WeaponTriggerType
{
    SemiAuto = 0,
    FullAuto = 1,
    Charge = 2,
}
