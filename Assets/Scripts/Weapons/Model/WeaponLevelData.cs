using UnityEngine;

[System.Serializable]
public class WeaponLevelData
{
    [Header("Identity")]
    [Tooltip("このレベルの表示名")]
    [SerializeField] private string displayName = "New Weapon";
    [Tooltip("このレベルの説明文")]
    [SerializeField] private string description;
    [Tooltip("このレベルのアイコン")]
    [SerializeField] private Sprite icon;

    [Header("Visual")]
    [Tooltip("このレベルの見た目")]
    [SerializeField] private WeaponVisual visual;
    [Tooltip("このレベルのサウンド")]
    [SerializeField] private SoundDB soundDB;

    [Header("Fire")]
    [Tooltip("このレベルの射撃方式")]
    [SerializeField] private FireModeData fireModeData;
    [Tooltip("このレベルのトリガーの挙動")]
    [SerializeField] private WeaponTriggerConfig trigger;

    [Header("Stats")]
    [Tooltip("このレベルの威力・精度")]
    [SerializeField] private DamageProfile damage;
    [Tooltip("このレベルの射撃リズム")]
    [SerializeField] private FireRhythmProfile fireRhythm;
    [Tooltip("このレベルの反動")]
    [SerializeField] private Recoil recoil;
    [Tooltip("このレベルの弾薬・リロード")]
    [SerializeField] private AmmoProfile ammo;

    public static WeaponLevelData Empty { get; } = new WeaponLevelData
    {
        visual = new WeaponVisual(),
        trigger = new WeaponTriggerConfig(),
        damage = DamageProfile.Empty,
        fireRhythm = FireRhythmProfile.Empty,
        recoil = Recoil.Empty,
        ammo = AmmoProfile.Empty,
    };

    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;

    public WeaponVisual Visual => visual;
    public GameObject WeaponModelPrefab => visual.WeaponModelPrefab;
    public SoundDB SoundDB => soundDB;

    public FireModeData FireModeData => fireModeData;
    public WeaponTriggerConfig Trigger => trigger;
    public bool AutoFire => trigger.AutoFire;
    public bool AutoReload => trigger.AutoReload;
    public WeaponTriggerType TriggerType => trigger.TriggerType;

    public DamageProfile DamageProfile => damage;
    public int Damage => damage.Damage;
    public float KnockbackForce => damage.KnockbackForce;
    public float SpreadAngle => damage.SpreadAngle;

    public float FireInterval => fireRhythm.FireInterval;
    public int BurstCount => fireRhythm.BurstCount;
    public float BurstInterval => fireRhythm.BurstInterval;
    public float ChargeTime => fireRhythm.ChargeTime;

    public int MagazineSize => ammo.MagazineSize;
    public float ReloadTime => ammo.ReloadTime;

    public Recoil Recoil => recoil ?? Recoil.Empty;
}
