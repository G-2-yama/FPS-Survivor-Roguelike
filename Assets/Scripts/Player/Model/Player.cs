using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// プレイヤーの装備と体力をまとめて保持するモデル。
/// 移動や入力の実処理は別クラスへ寄せ、ここでは他システムが参照する状態のみを管理する。
/// </summary>
public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private float exp;
    public float Exp => exp;
    private int pendingLevelUps;
    public int PendingLevelUps => pendingLevelUps;
    [SerializeField] private ExpManager expmanager;

    [SerializeField] private PlayerConfig config;
    public PlayerConfig Config => config;

    [SerializeField] private PlayerInventory inventory;
    public PlayerInventory Inventory => inventory;

    [SerializeField] private Sounder sounder;
    public Sounder Sounder => sounder;

    public TeamType TeamType => TeamType.Player;

    public event Action OnDeath;
    public event Action OnLevelUp;
    public event Action<float> OnExpGained;

    private bool IsWeaponSyncEnabled = false;
    public bool IsWeaponSync => IsWeaponSyncEnabled;
    public void SetWeaponSync(bool enabled)
    {
        IsWeaponSyncEnabled = enabled;
    }


    /// <summary>
    /// プレイヤーの体力モデル
    /// </summary>
    public Health Health { get; private set; }
    
    private int level = 1;
    public int Level => level;
    
    public PlayerStats Stats { get; private set; }

    public TimedBuffManager TimedBuffManager { get; private set; } 

    private void Awake()
    {
        InitializeIfNeeded();
    }
    public void InitializeIfNeeded()
    {
        if (Stats != null)
        {
            return;
        }

        Stats = new PlayerStats(config);
        Health = new Health(Stats.MaxHP);
        TimedBuffManager = new TimedBuffManager();
        Health.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (Health != null)
        {
            Health.OnDeath -= HandleDeath;
        }
    }

    public void TakeDamage(int damage, float knockbackForce)
    {
        Health.TakeDamage(damage);
    }
    public void AddExp(float amount)
    {
        // 経験値を加算
        exp += amount;
        OnExpGained?.Invoke(amount);
        sounder.Play(SoundCategory.GetExp);

        // レベルアップ処理
        if (exp >= expmanager.LevelUpRequiredExp)
        {
            exp -= expmanager.LevelUpRequiredExp;

            level++;
            pendingLevelUps++;
            expmanager.IncreaseRequiredExp();
            OnLevelUp?.Invoke();
        }
    }
    public void ConsumePendingLevelUp()
    {
        if (pendingLevelUps > 0)
        {
            pendingLevelUps--;
        }
    }

    private void HandleDeath() => OnDeath?.Invoke();
}
