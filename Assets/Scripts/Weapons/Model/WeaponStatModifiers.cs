public struct WeaponStatModifiers
{
    public float DamageMultiplier;
    public int FlatDamageBonus;
    public float ReloadTimeMultiplier;
    public float MagazineMultiplier;
    public float KnockbackBonus;
    public float AreaExtraLifetime;
    public int AreaDamagePerSecond;

    public static WeaponStatModifiers Default => new WeaponStatModifiers
    {
        DamageMultiplier = 1f,
        ReloadTimeMultiplier = 1f,
        MagazineMultiplier = 1f,
    };
}
