using System;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>プレイヤー専用の体力、強化値、成長状態と装備参照を管理する</summary>
[RequireComponent(typeof(PlayerConfig))]
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
    private PlayerProgression progression;
    private int pendingLevelUps;
    private bool isWeaponSyncEnabled;

    public PlayerConfig Config => config != null ? config : GetComponent<PlayerConfig>();
    public PlayerSettingsSnapshot Settings { get; private set; }
    public PlayerInventory Inventory => inventory;
    public Sounder Sounder => sounder;
    public TeamType TeamType => TeamType.Player;
    public PlayerStats Stats { get; private set; }
    public Health Health => Stats?.Health;
    public TimedBuffManager TimedBuffManager { get; private set; }
    public float Exp => progression?.Experience ?? 0f;
    public float LevelUpRequiredExp => progression?.RequiredExperience ?? Config.Progression.LevelUpRequiredExp;
    public int Level => progression?.Level ?? Config.Progression.InitialLevel;
    public int PendingLevelUps => pendingLevelUps;
    public bool IsWeaponSync => isWeaponSyncEnabled;
    public event Action OnDeath;
    public event Action OnLevelUp;
    public event Action<float> OnExpGained;

    /// <summary>装備中の武器状態を同期するか設定する</summary>
    public void SetWeaponSync(bool enabled) => isWeaponSyncEnabled = enabled;

    /// <summary>生成時にプレイヤー専用の実行時状態を作る</summary>
    private void Awake() => InitializeIfNeeded();

    /// <summary>呼び出し順にかかわらず設定を検証し、一度だけ状態を生成する</summary>
    public void InitializeIfNeeded()
    {
        if (Stats != null) return;
        config = Config;
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
        Settings = config.CreateRuntimeSnapshot();
        Stats = new PlayerStats(Settings.Stats);
        progression = new PlayerProgression(Settings.Progression);
        pendingLevelUps = progression.AddExperience(initialExperience);
        // MonoBehaviourをnewで生成せず、実際に更新されるコンポーネントを使用する
        TimedBuffManager = GetComponent<TimedBuffManager>();
        if (TimedBuffManager == null) TimedBuffManager = gameObject.AddComponent<TimedBuffManager>();
        Health.OnDeath += HandleDeath;
    }

    /// <summary>破棄前にイベント購読と設定の実行時複製を解放する</summary>
    private void OnDestroy()
    {
        if (Health != null) Health.OnDeath -= HandleDeath;
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
    private void HandleDeath() => OnDeath?.Invoke();
}
