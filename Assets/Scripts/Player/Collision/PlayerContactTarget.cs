using System;
using UnityEngine;

/// <summary>敵固有の制御から押し出しに必要な体と接触ダメージを分離する</summary>
[DisallowMultipleComponent]
public sealed class PlayerContactTarget : MonoBehaviour
{
    [SerializeField, PlayerSettingLabel("Body"), Tooltip("水平方向の位置補正を適用するRigidbody")]
    private Rigidbody body;
    [SerializeField, PlayerSettingLabel("Damage Source"), Tooltip("IContactDamageSourceを実装したコンポーネント")]
    private MonoBehaviour damageSource;
    [SerializeField, Min(0f), PlayerSettingLabel("Radius Override"), Tooltip("0なら通常Colliderから推定する 0より大きい場合はワールド座標の半径として使用する")]
    private float radiusOverride;
    private bool available = true;

    public Rigidbody Body => body;
    public bool IsAvailable => available && isActiveAndEnabled && body != null && body.gameObject.activeInHierarchy;
    public int ContactDamage => damageSource is IContactDamageSource source ? source.DamageAmount : 0;
    public event Action<PlayerContactTarget> OnUnavailable;

    /// <summary>手動で配置した対象のRigidbodyを補完する</summary>
    private void Awake()
    {
        if (body == null) body = GetComponent<Rigidbody>();
    }

    /// <summary>取得時に対象の参照を設定し、新しい活動期間を開始する</summary>
    public void BeginLife(Rigidbody targetBody, MonoBehaviour source)
    {
        Invalidate();
        body = targetBody;
        damageSource = source;
        available = true;
    }

    /// <summary>プール返却や無効化時にすべての接触領域へ追跡終了を通知する</summary>
    public void Invalidate()
    {
        available = false;
        OnUnavailable?.Invoke(this);
    }

    /// <summary>無効化時に古い接触状態を残さないよう通知する</summary>
    private void OnDisable() => Invalidate();

    /// <summary>手動配置した対象が再び有効になった際に使用可能にする</summary>
    private void OnEnable() => available = true;

    /// <summary>敵の通常Colliderからワールド座標の水平半径を推定する</summary>
    public float GetHorizontalRadius()
    {
        if (radiusOverride > 0f) return radiusOverride;
        float radius = 0f;
        foreach (Collider targetCollider in GetComponentsInChildren<Collider>())
        {
            if (targetCollider.isTrigger || !targetCollider.enabled) continue;
            Bounds bounds = targetCollider.bounds;
            radius = Mathf.Max(radius, Mathf.Max(bounds.extents.x, bounds.extents.z));
        }
        return radius;
    }
}
