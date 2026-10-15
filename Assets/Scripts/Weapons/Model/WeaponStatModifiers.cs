public struct WeaponStatModifiers
{
    public float DamageMultiplier;
    public float ReloadTimeMultiplier;
    public float MagazineMultiplier;

    public static WeaponStatModifiers Default => new WeaponStatModifiers
    {
        DamageMultiplier = 1f,
        ReloadTimeMultiplier = 1f,
        MagazineMultiplier = 1f,
    };
}
