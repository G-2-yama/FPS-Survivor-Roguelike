public interface IDamageable
{
    void TakeDamage(int damage);
    TeamType TeamType { get; }
}
