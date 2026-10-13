using UnityEngine;

/// <summary>プレイヤーの初期能力を保持し、実行中の強化値の生成元として使用する</summary>
[CreateAssetMenu(fileName = "PlayerStatsSettings", menuName = "Player/設定/ステータス")]
public sealed class PlayerStatsSettings : PlayerSettingsAsset
{
    [Header("Health")]
    [SerializeField, Min(1f), PlayerSettingLabel("Initial HP"), Tooltip("開始時の最大HPと現在HP 強化後の値はプレイヤーごとに保持する")]
    private int initialHP = 100;

    /// <summary>開始時の最大HPと現在HP 強化後の値はプレイヤーごとに保持する</summary>
    public int InitialHP => initialHP;

    [Header("Movement Stats")]
    [SerializeField, Min(0f), PlayerSettingLabel("Walk Speed"), Tooltip("通常移動の基準速度 単位はm/秒")]
    private float walkSpeed = 7.5f;

    /// <summary>通常移動の基準速度 単位はm/秒</summary>
    public float WalkSpeed => walkSpeed;

    [SerializeField, Min(0f), PlayerSettingLabel("Run Speed"), Tooltip("走行入力中の基準速度 単位はm/秒")]
    private float runSpeed = 10f;

    /// <summary>走行入力中の基準速度 単位はm/秒</summary>
    public float RunSpeed => runSpeed;

    [Header("Jump Stats")]
    [SerializeField, Min(0f), PlayerSettingLabel("Jump Force"), Tooltip("ジャンプ開始時の上向き速度 単位はm/秒")]
    private float jumpForce = 8f;

    /// <summary>ジャンプ開始時の上向き速度 単位はm/秒</summary>
    public float JumpForce => jumpForce;

    [Header("Dash Stats")]
    [SerializeField, Min(0f), PlayerSettingLabel("Dash Distance"), Tooltip("1回のダッシュで進む距離 単位はm")]
    private float dashDistance = 6f;

    /// <summary>1回のダッシュで進む距離 単位はm</summary>
    public float DashDistance => dashDistance;

    [SerializeField, Min(0.01f), PlayerSettingLabel("Dash Duration"), Tooltip("ダッシュの継続時間 単位は秒 距離と時間から速度を計算する")]
    private float dashDuration = 0.12f;

    /// <summary>ダッシュの継続時間 単位は秒 距離と時間から速度を計算する</summary>
    public float DashDuration => dashDuration;

    [SerializeField, Min(0f), PlayerSettingLabel("Dash Cooldown"), Tooltip("ダッシュ終了後から次の開始までの時間 単位は秒")]
    private float dashCooldown = 0.5f;

    /// <summary>ダッシュ終了後から次の開始までの時間 単位は秒</summary>
    public float DashCooldown => dashCooldown;

    [Header("Combat Stats")]
    [SerializeField, Min(0f), PlayerSettingLabel("Damage Multiplier"), Tooltip("武器の基礎ダメージに掛ける倍率 1で等倍")]
    private float damageMultiplier = 1f;

    /// <summary>武器の基礎ダメージに掛ける倍率 1で等倍</summary>
    public float DamageMultiplier => damageMultiplier;

    [SerializeField, Min(0f), PlayerSettingLabel("Knockback Force Multiplier"), Tooltip("武器のノックバック量に掛ける倍率 1で等倍")]
    private float knockbackForceMultiplier = 1f;

    /// <summary>武器のノックバック量に掛ける倍率 1で等倍</summary>


    [Header("Pickup")]
    [SerializeField, Min(0f), PlayerSettingLabel("Pickup Radius"), Tooltip("アイテムを引き寄せ始める距離")]
    private float pickupRadius = 3f;

    /// <summary>アイテムを引き寄せ始める距離</summary>
    public float PickupRadius => pickupRadius;
    public float KnockbackForceMultiplier => knockbackForceMultiplier;

    /// <summary>ステータスの数値と関連する条件を検証する</summary>
    public override bool Validate(out string error)
    {
        if (initialHP < 1) { error = "初期最大HPの値が有効範囲外です"; return false; }
        if (!IsFinite(walkSpeed) || walkSpeed < 0f) { error = "歩行速度の値が有効範囲外です"; return false; }
        if (!IsFinite(runSpeed) || runSpeed < 0f) { error = "走行速度の値が有効範囲外です"; return false; }
        if (!IsFinite(jumpForce) || jumpForce < 0f) { error = "ジャンプ初速の値が有効範囲外です"; return false; }
        if (!IsFinite(dashDistance) || dashDistance < 0f) { error = "ダッシュ距離の値が有効範囲外です"; return false; }
        if (!IsFinite(dashDuration) || dashDuration < 0.01f) { error = "ダッシュ時間の値が有効範囲外です"; return false; }
        if (!IsFinite(dashCooldown) || dashCooldown < 0f) { error = "ダッシュ待ち時間の値が有効範囲外です"; return false; }
        if (!IsFinite(damageMultiplier) || damageMultiplier < 0f) { error = "攻撃倍率の値が有効範囲外です"; return false; }
        if (!IsFinite(knockbackForceMultiplier) || knockbackForceMultiplier < 0f) { error = "ノックバック倍率の値が有効範囲外です"; return false; }
        if (!IsFinite(pickupRadius) || pickupRadius < 0f) { error = "Invalid pickup radius"; return false; }
        error = null;
        return true;
    }
}

