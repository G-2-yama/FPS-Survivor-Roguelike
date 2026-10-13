using UnityEngine;
using System;

/// <summary>体力、ダメージ、回復と死亡通知を管理する</summary>
public class Health
{
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }

    public bool IsDead => CurrentHP <= 0;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int, int> OnDamaged;
    public event Action<int, int, int> OnHealed;
    public event Action OnDeath;

    /// <summary>最大HPから生存状態の体力を生成する</summary>
    public Health(int maxHP)
    {
        MaxHP = maxHP;
        CurrentHP = maxHP;
    }

    /// <summary>有効なダメージを適用して体力変更と死亡を通知する</summary>
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

    /// <summary>生存中の体力を最大HPの範囲で回復する</summary>
    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;

        int previousHp = CurrentHP;
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        int appliedHeal = CurrentHP - previousHp;

        if (appliedHeal <= 0)
        {
            return;
        }

        OnHealed?.Invoke(appliedHeal, CurrentHP, MaxHP);
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
    }

    /// <summary>最大HPと現在HPを同じ増減量で変更し、有効範囲を維持する</summary>
    public void IncreaseHP(int amount)
    {
        bool wasDead = IsDead;
        int nextMaxHP = (int)Math.Max(1L, Math.Min(int.MaxValue, (long)MaxHP + amount));
        int appliedAmount = nextMaxHP - MaxHP;
        MaxHP = nextMaxHP;
        CurrentHP = wasDead ? 0 : (int)Math.Max(0L, Math.Min(MaxHP, (long)CurrentHP + appliedAmount));
        OnHealthChanged?.Invoke(CurrentHP, MaxHP);
        if (!wasDead && IsDead) OnDeath?.Invoke();
    }
}
