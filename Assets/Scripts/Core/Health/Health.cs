using UnityEngine;
using System;

public class Health
{
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }

    public bool IsDead => CurrentHP <= 0;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    public Health(int maxHP)
    {
        MaxHP = maxHP;
        CurrentHP = maxHP;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        CurrentHP = Mathf.Max(CurrentHP - damage, 0);
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
}
