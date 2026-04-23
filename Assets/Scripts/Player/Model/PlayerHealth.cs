using System;
using UnityEngine;

/// <summary>
/// プレイヤーの体力と被ダメージ判定を扱うモデル。
/// 移動や視点設定には依存せず、体力責務だけを持つ。
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    /// <summary>
    /// 体力初期化に使う初期HP
    /// </summary>
    [SerializeField] private int initialHP = 100;

    /// <summary>
    /// ダメージ判定で使用する所属チーム
    /// </summary>
    public TeamType TeamType => TeamType.Player;

    /// <summary>
    /// 体力が0になったときに通知するイベント
    /// </summary>
    public event Action OnDeath;

    /// <summary>
    /// プレイヤーの体力を管理するモデル
    /// </summary>
    public Health Health { get; private set; }

    /// <summary>
    /// 体力モデルを初期HPで生成し、死亡イベントを購読する
    /// </summary>
    private void Awake()
    {
        Health = new Health(initialHP);
        Health.OnDeath += HandleDeath;
    }

    /// <summary>
    /// ダメージを受ける処理
    /// </summary>
    public void TakeDamage(int damage)
    {
        Health.TakeDamage(damage);
    }

    /// <summary>
    /// 死亡したときに呼び出される処理
    /// </summary>
    private void HandleDeath()
    {
        OnDeath?.Invoke();
    }
}
