public interface IDamageable
{
    void TakeDamage(int damage, float knockbackForce = 0f);
    TeamType TeamType { get; }
}
