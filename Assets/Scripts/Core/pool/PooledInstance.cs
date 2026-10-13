using System;
using UnityEngine;

/// <summary>プール個体で実際に使う部品だけを初回生成時に確保する</summary>
public sealed class PooledInstance
{
    public GameObject GameObject { get; }
    public Transform Transform { get; }
    internal IPoolable Lifecycle { get; }
    internal DamageTargetBinding DamageBinding { get; }
    public PoolableObject Poolable { get; }
    public DamageBase Damage { get; }
    public MovementBase Movement { get; }
    public Enemy Enemy { get; }
    public enemyplojectileobject EnemyProjectile { get; }
    public PickupTriggerItem Pickup { get; }
    public Rigidbody Body { get; }
    public Tracer Tracer { get; }
    public PooledSoundPlayer SoundPlayer { get; }
    public DamagePopup DamagePopup { get; }

    /// <summary>ルート部品を一度だけ読み込み、型判定で用途別の参照に振り分ける</summary>
    internal PooledInstance(GameObject instance)
    {
        GameObject = instance;
        Transform = instance.transform;
        foreach (Component component in instance.GetComponents<Component>())
        {
            if (component == null) continue;
            // 同じ型が複数ある場合も、従来と同じ最初の部品を使う
            // 継承関係を持つ部品は複数の用途に該当するため、elseでつながない
            if (Poolable == null && component is PoolableObject poolable) Poolable = poolable;
            if (Lifecycle == null && component is IPoolable lifecycle) Lifecycle = lifecycle;
            if (DamageBinding == null && component is DamageTargetBinding binding) DamageBinding = binding;
            if (Damage == null && component is DamageBase damage) Damage = damage;
            if (Movement == null && component is MovementBase movement) Movement = movement;
            if (Enemy == null && component is Enemy enemy) Enemy = enemy;
            if (EnemyProjectile == null && component is enemyplojectileobject projectile) EnemyProjectile = projectile;
            if (Pickup == null && component is PickupTriggerItem pickup) Pickup = pickup;
            if (Body == null && component is Rigidbody body) Body = body;
            if (Tracer == null && component is Tracer tracer) Tracer = tracer;
            if (SoundPlayer == null && component is PooledSoundPlayer sound) SoundPlayer = sound;
            if (DamagePopup == null && component is DamagePopup popup) DamagePopup = popup;
        }
    }

    /// <summary>攻撃生成で必須となるダメージ部品を返す</summary>
    public DamageBase RequireDamage() => Damage != null ? Damage : throw Missing(nameof(DamageBase));

    /// <summary>ヒットスキャンの演出で必須となる軌跡弾を返す</summary>
    public Tracer RequireTracer() => Tracer != null ? Tracer : throw Missing(nameof(Tracer));

    /// <summary>効果音再生で必須となる音声部品を返す</summary>
    public PooledSoundPlayer RequireSoundPlayer() => SoundPlayer != null ? SoundPlayer : throw Missing(nameof(PooledSoundPlayer));

    /// <summary>ダメージ表示で必須となる表示部品を返す</summary>
    public DamagePopup RequireDamagePopup() => DamagePopup != null ? DamagePopup : throw Missing(nameof(DamagePopup));

    /// <summary>不足した部品と修正するPrefabを通知する</summary>
    private InvalidOperationException Missing(string componentName) =>
        new InvalidOperationException($"{GameObject.name} に必須の {componentName} がありません");
}
