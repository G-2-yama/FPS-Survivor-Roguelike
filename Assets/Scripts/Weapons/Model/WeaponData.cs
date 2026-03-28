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
    [SerializeField] private string displayName = "New Weapon";

    [Header("Attack")]
    [SerializeField] private WeaponAttackType attackType = WeaponAttackType.HitScan;
    [SerializeField] private WeaponTriggerType triggerType = WeaponTriggerType.FullAuto;
    [SerializeField, Min(0f)] private float damage = 10f;
    [SerializeField, Min(0.01f)] private float fireInterval = 0.12f;
    [SerializeField, Min(1)] private int projectilesPerShot = 1;
    [SerializeField, Range(0f, 45f)] private float spreadAngle = 0.5f;
    [SerializeField, Min(0f)] private float maxRange = 60f;

    [Header("Projectile")]
    [SerializeField, Min(0f)] private float projectileSpeed = 80f;
    [SerializeField, Min(0f)] private float projectileLifetime = 2f;

    [Header("Burst")]
    [SerializeField] private bool useBurst;
    [SerializeField, Min(1)] private int burstCount = 3;
    [SerializeField, Min(0f)] private float burstInterval = 0.05f;

    [Header("Ammo")]
    [SerializeField] private bool useMagazine = true;
    [SerializeField, Min(1)] private int magazineSize = 30;
    [SerializeField, Min(0f)] private float reloadTime = 1.6f;
    [SerializeField, Min(0)] private int maxReserveAmmo = 120;

    public string WeaponId => weaponId;
    public string DisplayName => displayName;

    public WeaponAttackType AttackType => attackType;
    public WeaponTriggerType TriggerType => triggerType;
    public float Damage => damage;
    public float FireInterval => fireInterval;
    public int ProjectilesPerShot => projectilesPerShot;
    public float SpreadAngle => spreadAngle;
    public float MaxRange => maxRange;

    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifetime => projectileLifetime;

    public bool UseBurst => useBurst;
    public int BurstCount => burstCount;
    public float BurstInterval => burstInterval;

    public bool UseMagazine => useMagazine;
    public int MagazineSize => magazineSize;
    public float ReloadTime => reloadTime;
    public int MaxReserveAmmo => maxReserveAmmo;

    public bool IsProjectileWeapon => attackType == WeaponAttackType.Projectile;
}

public enum WeaponAttackType
{
    HitScan = 0,
    Projectile = 1,
}

public enum WeaponTriggerType
{
    SemiAuto = 0,
    FullAuto = 1,
    Charge = 2,
}
