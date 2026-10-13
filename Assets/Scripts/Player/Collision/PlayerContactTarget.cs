using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>敵固有の制御から押し出しに必要な体と接触ダメージを分離する</summary>
[DisallowMultipleComponent]
public sealed class PlayerContactTarget : MonoBehaviour
{
    private static readonly Dictionary<Rigidbody, PlayerContactTarget> targetsByBody = new();

    /// <summary>プレイ開始時に前回の接触対象を破棄する</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => targetsByBody.Clear();
    [SerializeField, PlayerSettingLabel("Body"), Tooltip("水平方向の位置補正を適用するRigidbody")]
    private Rigidbody body;
    [SerializeField, PlayerSettingLabel("Damage Source"), Tooltip("IContactDamageSourceを実装したコンポーネント")]
    private MonoBehaviour damageSource;
    [SerializeField, Min(0f), PlayerSettingLabel("Radius Override"), Tooltip("0なら通常Colliderから推定する 0より大きい場合はワールド座標の半径として使用する")]
    private float radiusOverride;
    private bool available = true;
    private bool canBePushed = true;

    public Rigidbody Body => body;
    public bool IsAvailable => available && isActiveAndEnabled && body != null && body.gameObject.activeInHierarchy;
    public bool CanBePushed => canBePushed;
    public float RadiusOverride => radiusOverride;
    public int ContactDamage => damageSource is IContactDamageSource source ? source.DamageAmount : 0;
    public event Action<PlayerContactTarget> OnUnavailable;

    /// <summary>取得時に対象の参照と押し出し可否を設定し、新しい活動期間を開始する</summary>
    /// <param name="targetBody">水平位置を補正する対象のRigidbody</param>
    /// <param name="source">接触ダメージの取得元</param>
    /// <param name="pushable">プレイヤーの接触判定で押し出す場合はtrue</param>
    public void BeginLife(Rigidbody targetBody, MonoBehaviour source, bool pushable = true)
    {
        Invalidate();
        body = targetBody;
        damageSource = source;
        canBePushed = pushable;
        available = true;
        if (body != null) targetsByBody[body] = this;
    }

    /// <summary>プール返却や無効化時にすべての接触領域へ追跡終了を通知する</summary>
    public void Invalidate()
    {
        available = false;
        if (body != null && targetsByBody.TryGetValue(body, out PlayerContactTarget registered) && registered == this)
            targetsByBody.Remove(body);
        OnUnavailable?.Invoke(this);
    }

    /// <summary>無効化時に古い接触状態を残さないよう通知する</summary>
    private void OnDisable() => Invalidate();

    /// <summary>手動配置した対象が再び有効になった際に使用可能にする</summary>
    private void OnEnable() => available = true;

    /// <summary>Rigidbodyをキーとして登録済みの接触対象を取得する</summary>
    public static bool TryResolve(Rigidbody targetBody, out PlayerContactTarget target)
    {
        target = null;
        return targetBody != null && targetsByBody.TryGetValue(targetBody, out target);
    }

}
