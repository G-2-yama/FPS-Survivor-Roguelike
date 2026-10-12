using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// プレイヤーの装備と体力をまとめて保持するモデル
/// 移動や入力の実処理は別クラスへ寄せ、ここでは他システムが参照する状態のみを管理する
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
    /// <summary>装備中の武器状態を同期するか設定する</summary>
    /// <param name="enabled">武器同期を有効にするか</param>
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

    /// <summary>生成時にプレイヤーの体力と成長状態を初期化する</summary>
    private void Awake()
    {
        InitializeIfNeeded();
    }
    /// <summary>コントローラーから呼ばれた場合も含め、ランタイム状態を一度だけ生成する</summary>
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

    /// <summary>破棄前に体力イベントの購読を解除する</summary>
    private void OnDestroy()
    {
        if (Health != null)
        {
            Health.OnDeath -= HandleDeath;
        }
    }

    /// <summary>プレイヤーの体力にダメージを適用する</summary>
    /// <param name="damage">減少させる体力</param>
    /// <param name="knockbackForce">プレイヤーの体力モデルでは使用しない値</param>
    public void TakeDamage(int damage, float knockbackForce)
    {
        Health.TakeDamage(damage);
    }

    /// <summary>経験値を加算し、必要量に達した場合はレベルアップ通知を行う</summary>
    /// <param name="amount">加算する経験値</param>
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
    /// <summary>保留中のレベルアップ選択を一つ消費する</summary>
    public void ConsumePendingLevelUp()
    {
        if (pendingLevelUps > 0)
        {
            pendingLevelUps--;
        }
    }

    /// <summary>体力の死亡通知をプレイヤーの購読者へ伝える</summary>
    private void HandleDeath() => OnDeath?.Invoke();
}
