public class PlayerStats
{
    // Vitality
    public int MaxHP { get; private set; }

    // Movement
    public float WalkSpeed { get; private set; }
    public float RunSpeed { get; private set; }

    // Jump
    public float JumpForce { get; private set; }

    // Dash
    public float DashDistance { get; private set; }
    public float DashDuration { get; private set; }
    public float DashCooldown { get; private set; }

    // Combat
    public float DamageMultiplier { get; private set; }
    public float KnockbackForceMultiplier { get; private set; }

    public PlayerStats(PlayerConfig baseStats)
    {
        MaxHP = baseStats.InitialHP;
        WalkSpeed = baseStats.WalkSpeed;
        RunSpeed = baseStats.RunSpeed;
        JumpForce = baseStats.JumpForce;
        DashDistance = baseStats.DashDistance;
        DashDuration = baseStats.DashDuration;
        DashCooldown = baseStats.DashCooldown;
        DamageMultiplier = 1f;
        KnockbackForceMultiplier = 1f;
    }

    public void AddMaxHP(int value)
    {
        MaxHP += value;
    }

    public void AddWalkSpeed(float value)
    {
        WalkSpeed += value;
    }

    public void AddRunSpeed(float value)
    {
        RunSpeed += value;
    }

    public void AddJumpForce(float value)
    {
        JumpForce += value;
    }

    public void AddDashDistance(float value)
    {
        DashDistance += value;
    }

    public void AddDashDuration(float value)
    {
        DashDuration += value;
    }

    public void AddDashCooldown(float value)
    {
        DashCooldown += value;
    }

    public void AddDamageMultiplier(float multiplier)
    {
        DamageMultiplier += multiplier;
    }

    public void AddKnockbackForceMultiplier(float multiplier)
    {
        KnockbackForceMultiplier += multiplier;
    }
}