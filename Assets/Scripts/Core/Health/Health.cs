using UnityEngine;
using System;

public class Health
{
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }

    public bool IsDead => CurrentHP <= 0;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int, int> OnDamaged;
    public event Action OnDeath;

    public Health(int maxHP)
    {
        MaxHP = maxHP;
        CurrentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead || damage <= 0) return;

        int previousHp = CurrentHP;
        CurrentHP = Mathf.Max(CurrentHP - damage, 0);
        int appliedDamage = previousHp - CurrentHP;

        if (appliedDamage <= 0)
        {
            return;
        }

        OnDamaged?.Invoke(appliedDamage, CurrentHP, MaxHP);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);

        if (CurrentHP == 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;

        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    public void IncreaseHP(int amount)
    {
        MaxHP += amount;
        CurrentHP += amount;
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }
}
