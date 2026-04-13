using UnityEngine;
using System;

/// <summary>
/// プレイヤーの体力と被ダメージ判定を扱うモデル
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    /// <summary>
    /// 初期HPなど、体力初期化にも利用するプレイヤー設定
    /// </summary>
    [SerializeField] private PlayerConfig config;
    public PlayerConfig Config => config;

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
        Health = new Health(config.InitialHP);
        Health.OnDeath += HandleDeath;
    }

    /// <summary>
    /// ダメージを受ける処理
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
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
