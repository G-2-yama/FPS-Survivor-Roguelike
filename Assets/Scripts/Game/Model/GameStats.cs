public class GameStats
{
    public int KillCount { get; private set; }
    public int FireCount { get; private set; }
    public int TotalDamage { get; private set; }
    public int HitCount { get; private set; }

    public float HitsPerFire => FireCount == 0 ? 0f : (float)HitCount / FireCount;

    public GameStats()
    {
        Enemy.OnAnyDamaged += HandleEnemyDamaged;
        Enemy.OnAnyKilled += HandleEnemyKilled;
        Weapon.OnAnyFired += HandleWeaponFired;
    }

    public void Dispose()
    {
        Enemy.OnAnyDamaged -= HandleEnemyDamaged;
        Enemy.OnAnyKilled -= HandleEnemyKilled;
        Weapon.OnAnyFired -= HandleWeaponFired;
    }

    private void HandleEnemyDamaged(int appliedDamage)
    {
        TotalDamage += appliedDamage;
        HitCount++;
    }

    private void HandleEnemyKilled() => KillCount++;

    private void HandleWeaponFired() => FireCount++;
}
