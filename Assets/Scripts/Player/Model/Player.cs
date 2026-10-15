using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>プレイヤー専用の体力、強化値、成長状態と装備参照を管理する</summary>
[RequireComponent(typeof(PlayerConfig))]
[RequireComponent(typeof(DamageTargetBinding))]
public class Player : MonoBehaviour, IDamageable
{
    [FormerlySerializedAs("exp")]
    [SerializeField, Min(0f), PlayerSettingLabel("Initial Experience"), Tooltip("生成時に与える経験値 実行中の残余経験値とは別の初期値")]
    private float initialExperience;
    [SerializeField, PlayerSettingLabel("Config"), Tooltip("5分類の設定SOを参照するコンポーネント 未指定なら同じオブジェクトから取得する")]
    private PlayerConfig config;
    [SerializeField, PlayerSettingLabel("Inventory"), Tooltip("プレイヤーの装備スロットを管理するコンポーネント")]
    private PlayerInventory inventory;
    [SerializeField, PlayerSettingLabel("Sounder"), Tooltip("プレイヤーの効果音を再生するコンポーネント")]
    private Sounder sounder;
    [SerializeField, PlayerSettingLabel("Timed Buff Manager"), Tooltip("プレイヤーの一時強化を管理するコンポーネント")]
    private TimedBuffManager timedBuffManager;
    [SerializeField, PlayerSettingLabel("Attribute Catalog"), Tooltip("属性を数えるときに使う属性の一覧")]
    private AttributeCatalog attributeCatalog;
    private PlayerProgression progression;
    private AttributeEffectRunner attributeEffectRunner;
    private int pendingLevelUps;
    private bool isWeaponSyncEnabled;
    private CharacterController bodyController;
    private readonly List<WeaponEffect> weaponEffects = new List<WeaponEffect>();
    private readonly HashSet<object> movementLocks = new HashSet<object>();

    public PlayerConfig Config => config;
    public PlayerSettingsSnapshot Settings { get; private set; }
    public PlayerInventory Inventory => inventory;
    public Sounder Sounder => sounder;
    public TeamType TeamType => TeamType.Player;
    public PlayerStats Stats { get; private set; }
    public Health Health => Stats?.Health;
    public TimedBuffManager TimedBuffManager { get; private set; }
    public AttributeTracker Attributes { get; private set; }
    public float Exp => progression?.Experience ?? 0f;
    public float LevelUpRequiredExp => progression?.RequiredExperience ?? Config.Progression.LevelUpRequiredExp;
    public int Level => progression?.Level ?? Config.Progression.InitialLevel;
    public int PendingLevelUps => pendingLevelUps;
    public bool IsWeaponSync => isWeaponSyncEnabled;
    public IReadOnlyList<WeaponEffect> WeaponEffects => weaponEffects;
    public bool IsStationary { get; private set; }
    public bool IsMovementLocked => movementLocks.Count > 0;
    /// <summary>現在の姿勢を反映した本体の足元位置を取得する</summary>
    public Vector3 FootPosition
    {
        get
        {
            Vector3 position = transform.position;
            // developの生成位置に合わせ、姿勢ごとに変化する本体境界の下端を使う
            if (bodyController != null) position.y = bodyController.bounds.min.y;
            return position;
        }
    }
    public event Action OnDeath;
    public event Action OnLevelUp;
    public event Action<float> OnExpGained;

    /// <summary>装備中の武器状態を同期するか設定する</summary>
    public void SetWeaponSync(bool enabled) => isWeaponSyncEnabled = enabled;

    public void AddWeaponEffect(WeaponEffect effect)
    {
        if (effect == null) return;
        weaponEffects.Add(effect);
        RefreshWeaponStats();
    }

    public void RemoveWeaponEffect(WeaponEffect effect)
    {
        if (!weaponEffects.Remove(effect)) return;
        RefreshWeaponStats();
    }

    private void RefreshWeaponStats()
    {
        foreach (Weapon weapon in inventory.WeaponSlots.Values)
        {
            weapon.RefreshStats();
        }
    }

    internal void SetStationary(bool stationary) => IsStationary = stationary;

    public void AddMovementLock(object key)
    {
        if (key == null) return;
        movementLocks.Add(key);
    }

    public void RemoveMovementLock(object key)
    {
        if (key == null) return;
        movementLocks.Remove(key);
    }

    /// <summary>衝突初期化時に本体参照を関連付け、攻撃ごとのコンポーネント検索を避ける</summary>
    internal void SetBodyController(CharacterController controller) => bodyController = controller;

    /// <summary>生成時にプレイヤー専用の実行時状態を作る</summary>
    private void Awake() => InitializeIfNeeded();

    /// <summary>呼び出し順にかかわらず設定を検証し、一度だけ状態を生成する</summary>
    public void InitializeIfNeeded()
    {
        if (Stats != null) return;
        if (config == null)
        {
            Debug.LogError("プレイヤー設定の入口がありません", this);
            enabled = false;
            return;
        }
        if (!config.Validate(out string error))
        {
            Debug.LogError(error, this);
            enabled = false;
            return;
        }
        if (timedBuffManager == null)
        {
            Debug.LogError("TimedBuffManager が設定されていません", this);
            enabled = false;
            return;
        }
        Settings = config.CreateRuntimeSnapshot();
        Stats = new PlayerStats(Settings.Stats);
        progression = new PlayerProgression(Settings.Progression);
        pendingLevelUps = progression.AddExperience(initialExperience);
        // MonoBehaviourをnewで生成せず、実際に更新されるコンポーネントを使用する
        TimedBuffManager = timedBuffManager;
        Attributes = new AttributeTracker(inventory, attributeCatalog);
        attributeEffectRunner = new AttributeEffectRunner(this, Attributes);
        Health.OnDeath += HandleDeath;
    }

    /// <summary>破棄前にイベント購読と設定の実行時複製を解放する</summary>
    private void OnDestroy()
    {
        if (Health != null) Health.OnDeath -= HandleDeath;
        attributeEffectRunner?.Dispose();
        Attributes?.Dispose();
        Settings?.Dispose();
    }

    /// <summary>プレイヤーの体力にダメージを適用する</summary>
    public void TakeDamage(int damage, float knockbackForce) => Health?.TakeDamage(damage);

    /// <summary>経験値と全レベルアップを確定してからUIへ通知する</summary>
    public void AddExp(float amount)
    {
        if (progression == null || amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount)) return;
        int gainedLevels = progression.AddExperience(amount);
        pendingLevelUps += gainedLevels;
        for (int i = 0; i < gainedLevels; i++) OnLevelUp?.Invoke();
        OnExpGained?.Invoke(amount);
        sounder?.Play(SoundCategory.GetExp);
    }

    /// <summary>保留中のレベルアップ選択を一つ消費する</summary>
    public void ConsumePendingLevelUp()
    {
        if (pendingLevelUps > 0) pendingLevelUps--;
    }

    /// <summary>体力の死亡通知をプレイヤーの購読者へ伝える</summary>
    private void HandleDeath()
    {
        movementLocks.Clear();
        OnDeath?.Invoke();
    }
}
