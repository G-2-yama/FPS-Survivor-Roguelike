using UnityEngine;

[System.Serializable]
public sealed class WeaponStats
{
    [SerializeField] private DamageProfile damage;
    [SerializeField] private FireRhythmProfile fireRhythm;
    [SerializeField] private RecoilProfile recoil;
    [SerializeField] private AmmoProfile ammo;

    public int MagazineSize => ammo.MagazineSize;
    public float FireInterval => fireRhythm.FireInterval;
    public int BurstCount => fireRhythm.BurstCount;
    public float BurstInterval => fireRhythm.BurstInterval;
    public float RecoilX => recoil.RecoilX;
    public float RecoilY => recoil.RecoilY;
    public float ReloadTime => ammo.ReloadTime;
    public float RecoilRecoverySpeed => recoil.RecoverySpeed;
    public float SpreadAngle => damage.SpreadAngle;
    public int Damage => damage.Damage;
}

/// <summary>
/// 攻撃威力・精度。生成後は変更不可
/// </summary>
[System.Serializable]
public sealed class DamageProfile
{
    [SerializeField] private int damage;
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
    [SerializeField] private float fireInterval;
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
public sealed class RecoilProfile
{
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoverySpeed;

    public float RecoilX       => recoilX;
    public float RecoilY       => recoilY;
    public float RecoverySpeed => recoverySpeed;

    private RecoilProfile() { }
}

/// <summary>
/// 弾薬・リロード。生成後は変更不可。
/// </summary>
[System.Serializable]
public sealed class AmmoProfile
{
    [SerializeField] private int   magazineSize = 1;
    [SerializeField] private float reloadTime   = 1.0f;

    public int   MagazineSize => magazineSize;
    public float ReloadTime   => reloadTime;

    private AmmoProfile() { }
}