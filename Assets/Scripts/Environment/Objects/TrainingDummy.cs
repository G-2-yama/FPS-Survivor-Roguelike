using UnityEngine;
using System;
using Unity.VisualScripting;

public class TrainingDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private int initialHP = 100;
    public TeamType TeamType => TeamType.Player;

    /// <summary>
    /// 体力を管理するモデル
    /// </summary>
    public Health Health { get; private set; }

    private void Awake()
    {
        Health = new Health(initialHP);
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
        Destroy(this.gameObject);
    }
}
