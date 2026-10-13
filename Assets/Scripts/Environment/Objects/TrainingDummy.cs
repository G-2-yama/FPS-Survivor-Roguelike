using UnityEngine;
using System;
using Unity.VisualScripting;
/// <summary>攻撃検証用の対象の体力を管理する</summary>

[RequireComponent(typeof(DamageTargetBinding))]
public class TrainingDummy : MonoBehaviour, IDamageable
{
    [SerializeField] private int initialHP = 100;
    public TeamType TeamType => TeamType.Enemy;

    /// <summary>
    /// 体力を管理するモデル
    /// </summary>
    public Health Health { get; private set; }
    /// <summary>初回生成時に必要な参照と状態を初期化する</summary>

    private void Awake()
    {
        Health = new Health(initialHP);
        Health.OnDeath += HandleDeath;
    }

    /// <summary>
    /// ダメージを受ける処理
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    /// <param name="knockbackForce">受けるノックバック力</param>
    public void TakeDamage(int damage, float knockbackForce)
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
