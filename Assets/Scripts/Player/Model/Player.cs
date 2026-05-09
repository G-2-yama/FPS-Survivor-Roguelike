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
    [SerializeField] private PlayerConfig config;
    public PlayerConfig Config => config;

    [SerializeField] private PlayerInventory inventory;
    public PlayerInventory Inventory => inventory;

    public TeamType TeamType => TeamType.Player;

    public event Action OnDeath;
    public event Action OnLevelUp;


    /// <summary>
    /// プレイヤーの体力モデル
    /// </summary>
    public Health Health { get; private set; }
    
    private int level = 1;
    public int Level => level;

    private void Awake()
    {
        int initialHp = config != null ? config.InitialHP : 100;
        Health = new Health(initialHp);
        Health.OnDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        if (Health != null)
        {
            Health.OnDeath -= HandleDeath;
        }
    }

    public void TakeDamage(int damage)
    {
        Health.TakeDamage(damage);
    }

    public void LevelUp()
    {
        level++;
        OnLevelUp?.Invoke();
    }

    private void HandleDeath() => OnDeath?.Invoke();
}
