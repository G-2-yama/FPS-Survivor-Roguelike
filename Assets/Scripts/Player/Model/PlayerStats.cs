using UnityEngine;

/// <summary>共有SOの初期値から生成するプレイヤー専用の強化後ステータス</summary>
public sealed class PlayerStats
{
    /// <summary>このプレイヤーの体力状態を管理する</summary>
    public Health Health { get; }
    public int MaxHP => Health.MaxHP;
    public float WalkSpeed { get; private set; }
    public float RunSpeed { get; private set; }
    public float JumpForce { get; private set; }
    public float DashDistance { get; private set; }
    public float DashDuration { get; private set; }
    public float DashCooldown { get; private set; }
    public float DamageMultiplier { get; private set; }
    public float KnockbackForceMultiplier { get; private set; }

    /// <summary>初期値をコピーし、共有SOを書き換えずに強化できる状態を作る</summary>
    public PlayerStats(PlayerStatsSettings baseStats)
    {
        Health = new Health(baseStats.InitialHP);
        WalkSpeed = baseStats.WalkSpeed;
        RunSpeed = baseStats.RunSpeed;
        JumpForce = baseStats.JumpForce;
        DashDistance = baseStats.DashDistance;
        DashDuration = baseStats.DashDuration;
        DashCooldown = baseStats.DashCooldown;
        DamageMultiplier = baseStats.DamageMultiplier;
        KnockbackForceMultiplier = baseStats.KnockbackForceMultiplier;
    }

    /// <summary>体力モデルを唯一の最大HPとして更新する</summary>
    public void AddMaxHP(int value) => Health.IncreaseHP(value);
    /// <summary>歩行速度を加算し、負の速度を防ぐ</summary>
    public void AddWalkSpeed(float value) => WalkSpeed = Mathf.Max(0f, WalkSpeed + value);
    /// <summary>走行速度を加算し、負の速度を防ぐ</summary>
    public void AddRunSpeed(float value) => RunSpeed = Mathf.Max(0f, RunSpeed + value);
    /// <summary>ジャンプ初速を加算し、負の初速を防ぐ</summary>
    public void AddJumpForce(float value) => JumpForce = Mathf.Max(0f, JumpForce + value);
    /// <summary>ダッシュ距離を加算する</summary>
    public void AddDashDistance(float value) => DashDistance = Mathf.Max(0f, DashDistance + value);
    /// <summary>ダッシュ時間を加算し、速度計算のゼロ除算を防ぐ</summary>
    public void AddDashDuration(float value) => DashDuration = Mathf.Max(0.01f, DashDuration + value);
    /// <summary>ダッシュ待ち時間を加算する</summary>
    public void AddDashCooldown(float value) => DashCooldown = Mathf.Max(0f, DashCooldown + value);
    /// <summary>攻撃倍率を加算する</summary>
    public void AddDamageMultiplier(float value) => DamageMultiplier = Mathf.Max(0f, DamageMultiplier + value);
    /// <summary>ノックバック倍率を加算する</summary>
    public void AddKnockbackForceMultiplier(float value) => KnockbackForceMultiplier = Mathf.Max(0f, KnockbackForceMultiplier + value);
}