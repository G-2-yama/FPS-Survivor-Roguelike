using UnityEngine;

/// <summary>
/// 武器ごとの差分パラメータを保持するデータ定義
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
    [SerializeField] private SoundDB soundDB;
    public SoundDB SoundDB => soundDB;

    [Header("Classification")]
    [SerializeField] private WeaponClassification classification;
    public WeaponClassification Classification => classification;

    [Header("Trigger")]
    [SerializeField] private WeaponTriggerConfig trigger;
    public WeaponTriggerConfig Trigger => trigger;

    [Header("Stats")]
    [SerializeField] private DamageProfile damage;
    [SerializeField] private FireRhythmProfile fireRhythm;
    [SerializeField] private Recoil recoil;
    [SerializeField] private AmmoProfile ammo;

    [Header("Level")]
    [SerializeField] private WeaponData nextLevelData;
    public WeaponData NextLevelData => nextLevelData;

    // =========================
    // Trigger
    // =========================

    public bool AutoFire => trigger.AutoFire;
    public bool AutoReload => trigger.AutoReload;
    public WeaponTriggerType TriggerType => trigger.TriggerType;

    // =========================
    // Classification
    // =========================

    public WeaponType WeaponType => classification.WeaponType;
    public FireModeData FireModeData => classification.FireModeData;

    // =========================
    // Visual
    // =========================

    public GameObject WeaponModelPrefab => visual.WeaponModelPrefab;

    // =========================
    // Identity
    // =========================

    public Sprite Icon => identity.Icon;
    public string DisplayName => identity.DisplayName;
    public string WeaponId => identity.WeaponId;

    // =========================
    // Damage
    // =========================

    public int Damage => damage.Damage;
    public float SpreadAngle => damage.SpreadAngle;

    // =========================
    // Fire Rhythm
    // =========================

    public float FireInterval => fireRhythm.FireInterval;
    public int BurstCount => fireRhythm.BurstCount;
    public float BurstInterval => fireRhythm.BurstInterval;

    // =========================
    // Ammo
    // =========================

    public int MagazineSize => ammo.MagazineSize;
    public float ReloadTime => ammo.ReloadTime;

    // =========================
    // Recoil
    // =========================

    public Recoil Recoil => recoil ?? Recoil.Empty;
}

/// <summary>
/// 武器の識別・UI表示に関するデータ
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
/// </summary>
[System.Serializable]
public class WeaponVisual
{
    [SerializeField] private GameObject weaponModelPrefab;
    public GameObject WeaponModelPrefab => weaponModelPrefab;
}

/// <summary>
/// 射撃トリガーの挙動に関するデータ
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

/// <summary>
/// 攻撃威力・精度
/// </summary>
[System.Serializable]
public sealed class DamageProfile
{
    public static DamageProfile Empty { get; } = new DamageProfile();

    [SerializeField] private int damage = 1;
    [SerializeField, Range(0f, 45f)] private float spreadAngle;

    public int Damage => damage;
    public float SpreadAngle => spreadAngle;

    private DamageProfile() { }
}

/// <summary>
/// 射撃リズム
/// </summary>
[System.Serializable]
public sealed class FireRhythmProfile
{
    public static FireRhythmProfile Empty { get; } = new FireRhythmProfile();

    [SerializeField] private float fireInterval = 0.1f;
    [SerializeField] private int burstCount = 1;
    [SerializeField] private float burstInterval = 0.05f;

    public float FireInterval => fireInterval;
    public int BurstCount => burstCount;
    public float BurstInterval => burstInterval;

    private FireRhythmProfile() { }
}

/// <summary>
/// 反動
/// </summary>
[System.Serializable]
public sealed class Recoil
{
    public static Recoil Empty { get; } = new Recoil();

    [SerializeField] private float pitchKick = 1.2f;
    [SerializeField] private float yawKick = 0.4f;
    [SerializeField] private float yawRandomness = 1f;
    [SerializeField] private float returnStrength = 20f;
    [SerializeField] private float damping = 18f;
    [SerializeField] private float maxPitch = 20f;

    public float PitchKick => pitchKick;
    public float YawKick => yawKick;
    public float YawRandomness => yawRandomness;
    public float ReturnStrength => returnStrength;
    public float Damping => damping;
    public float MaxPitch => maxPitch;

    private Vector2 recoilOffset = Vector2.zero;
    private Vector2 recoilVelocity = Vector2.zero;

    public Vector2 RecoilOffset => recoilOffset;

    /// <summary>
    /// 発射時に反動を加える
    /// </summary>
    public void AddRecoil(float recoilMultiplier = 1f)
    {
        float yaw = Random.Range(-YawKick, YawKick) * YawRandomness;

        recoilOffset.y += PitchKick * recoilMultiplier;
        recoilOffset.x += yaw * recoilMultiplier;
    }

    /// <summary>
    /// 毎フレーム更新
    /// </summary>
    public Vector2 Tick(float deltaTime)
    {
        Vector2 accel =
            (-ReturnStrength * recoilOffset) -
            (Damping * recoilVelocity);

        recoilVelocity += accel * deltaTime;
        recoilOffset += recoilVelocity * deltaTime;

        recoilOffset.y =
            Mathf.Clamp(recoilOffset.y, -MaxPitch, MaxPitch);

        return recoilOffset;
    }
}

/// <summary>
/// 弾薬・リロード
/// </summary>
[System.Serializable]
public sealed class AmmoProfile
{
    public static AmmoProfile Empty { get; } = new AmmoProfile();

    [SerializeField] private int magazineSize = 1;
    [SerializeField] private float reloadTime = 1.0f;

    public int MagazineSize => magazineSize;
    public float ReloadTime => reloadTime;

    private AmmoProfile() { }
}