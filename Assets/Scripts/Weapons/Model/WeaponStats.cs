using UnityEngine;

[System.Serializable]
public class WeaponStats
{
    public int Damage;
    public float FireInterval;
    public float RecoilX;
    public float RecoilY;
    public float RecoilRecoverySpeed;
    public int MagazineSize = 1;
    public float ReloadTime = 1.0f;
    public int BurstCount = 1;
    public float BurstInterval = 0.05f;
    [SerializeField, Range(0f, 45f)] public float SpreadAngle = 0.0f;

    public WeaponStats Clone()
    {
        return new WeaponStats
        {
            Damage = Damage,
            FireInterval = FireInterval,
            SpreadAngle = SpreadAngle,
            RecoilX = RecoilX,
            RecoilY = RecoilY,
            MagazineSize = MagazineSize,
            ReloadTime = ReloadTime,
            RecoilRecoverySpeed = RecoilRecoverySpeed,
            BurstCount = BurstCount,
            BurstInterval = BurstInterval,
        };
    }
}