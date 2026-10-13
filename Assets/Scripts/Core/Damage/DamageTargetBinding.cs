using UnityEngine;
using System;

/// <summary>同じGameObjectのダメージ対象とColliderを初回生成時に結び付ける</summary>
[DisallowMultipleComponent]
public sealed class DamageTargetBinding : MonoBehaviour
{
    [SerializeField, Tooltip("IDamageableを実装する同じGameObjectのコンポーネント")]
    private MonoBehaviour targetSource;
    [SerializeField, Tooltip("このダメージ対象に対応する同じGameObjectのCollider")]
    private Collider[] targetColliders;
    private IDamageable target;
    public IDamageable Target => targetSource != null ? target : null;
    public bool IsRegistered { get; private set; }
    public event Action<IDamageable> OnUnavailable;

    /// <summary>未設定の参照を初回生成時だけ補完する</summary>
    private void Awake()
    {
        // 親や子の対象には拡張せず従来のGetComponentと同じ命中範囲を維持する
        if (targetSource == null) targetSource = GetComponent<IDamageable>() as MonoBehaviour;
        if (targetColliders == null || targetColliders.Length == 0) targetColliders = GetComponents<Collider>();
        target = targetSource as IDamageable;
        if (target == null)
        {
            Debug.LogError("ダメージ対象が設定されていません", this);
            enabled = false;
        }
    }

    /// <summary>シーン配置または有効化時に登録する</summary>
    private void OnEnable() => Register();

    /// <summary>無効化時にColliderの登録を解除する</summary>
    private void OnDisable() => Unregister();

    /// <summary>プール再取得時にも保持済みの参照で登録する</summary>
    public void Register()
    {
        if (!isActiveAndEnabled || Target == null || IsRegistered) return;
        IsRegistered = true;
        foreach (Collider collider in targetColliders) DamageTargetRegistry.Register(collider, this);
    }

    /// <summary>プール返却時に有効状態を維持する個体も登録から除外する</summary>
    public void Unregister()
    {
        if (!IsRegistered) return;
        IsRegistered = false;
        foreach (Collider collider in targetColliders) DamageTargetRegistry.Unregister(collider, this);
        OnUnavailable?.Invoke(target);
    }
}
