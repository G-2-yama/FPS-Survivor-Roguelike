using UnityEngine;

[System.Serializable]
public sealed class WeaponStats
{
    public static WeaponStats Empty { get; } = new WeaponStats();

    [SerializeField] private DamageProfile damage;
    [SerializeField] private FireRhythmProfile fireRhythm;
    [SerializeField] private Recoil recoil;
    public Recoil Recoil => recoil ?? Recoil.Empty;
    [SerializeField] private AmmoProfile ammo;

    public int MagazineSize => ammo.MagazineSize;
    public float FireInterval => fireRhythm.FireInterval;
    public int BurstCount => fireRhythm.BurstCount;
    public float BurstInterval => fireRhythm.BurstInterval;
    public float ReloadTime => ammo.ReloadTime;

    public float SpreadAngle => damage.SpreadAngle;
    public int Damage => damage.Damage;
}

/// <summary>
/// 攻撃威力・精度。生成後は変更不可
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
/// 射撃リズム。生成後は変更不可。
/// </summary>
[System.Serializable]
public sealed class FireRhythmProfile
{
    public static FireRhythmProfile Empty { get; } = new FireRhythmProfile();

    [SerializeField] private float fireInterval = 0.1f;
    [SerializeField] private int   burstCount    = 1;
    [SerializeField] private float burstInterval = 0.05f;

    public float FireInterval  => fireInterval;
    public int   BurstCount    => burstCount;
    public float BurstInterval => burstInterval;

    private FireRhythmProfile() { }
}

/// <summary>
/// 反動。生成後は変更不可。
/// </summary>
[System.Serializable]
public sealed class Recoil
{
    public static Recoil Empty { get; } = new Recoil();

    [SerializeField] private float pitchKick;
    public float PitchKick => pitchKick;
    [SerializeField] private float yawKick;
    public float YawKick => yawKick;
    [SerializeField] private float yawRandomness;
    public float YawRandomness => yawRandomness;
    [SerializeField] private float returnStrength;
    public float ReturnStrength => returnStrength;
    [SerializeField] private float damping;
    public float Damping => damping;
    [SerializeField] private float maxPitch;
    public float MaxPitch => maxPitch;

    private Vector2 recoilOffset = Vector2.zero;
    public Vector2 RecoilOffset => recoilOffset;
    private Vector2 recoilVelocity = Vector2.zero;

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
    /// 毎フレーム更新（回復処理）
    /// </summary>
    public Vector2 Tick(float deltaTime)
    {
        float dt = deltaTime;

        Vector2 accel = (-ReturnStrength * recoilOffset) - (Damping * recoilVelocity);
        
        recoilVelocity += accel * dt;
        recoilOffset += recoilVelocity * dt;

        recoilOffset.y = Mathf.Clamp(recoilOffset.y, -MaxPitch, MaxPitch);

        return recoilOffset;
    }
}

/// <summary>
/// 弾薬・リロード。生成後は変更不可。
/// </summary>
[System.Serializable]
public sealed class AmmoProfile
{
    public static AmmoProfile Empty { get; } = new AmmoProfile();

    [SerializeField] private int   magazineSize = 1;
    [SerializeField] private float reloadTime   = 1.0f;

    public int   MagazineSize => magazineSize;
    public float ReloadTime   => reloadTime;

    private AmmoProfile() { }
}