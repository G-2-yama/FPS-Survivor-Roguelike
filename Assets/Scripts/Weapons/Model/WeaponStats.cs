[System.Serializable]
public class WeaponStats
{
    public int Damage;
    public float FireInterval;
    public float SpreadAngle;
    public float RecoilX;
    public float RecoilY;
    public int MagazineSize;
    public float ReloadTime;
    public float RecoilRecoverySpeed;

    public void Add(WeaponStats bonusStats)
    {
        Damage += bonusStats.Damage;
        FireInterval += bonusStats.FireInterval;
        SpreadAngle += bonusStats.SpreadAngle;
        RecoilX += bonusStats.RecoilX;
        RecoilY += bonusStats.RecoilY;
        MagazineSize += bonusStats.MagazineSize;
        ReloadTime += bonusStats.ReloadTime;
        RecoilRecoverySpeed += bonusStats.RecoilRecoverySpeed;
    }
}