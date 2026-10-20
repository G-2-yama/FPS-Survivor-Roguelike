using UnityEngine;
using System;
using Unity.VisualScripting;
/// <summary>攻撃検証用の対象の体力を管理する</summary>

[RequireComponent(typeof(DamageTargetBinding))]
public class TrainingDummy : MonoBehaviour, IDamageable
{
    [Tooltip("体力の最大値")]
    [SerializeField] private int initialHP = 100;

    [Tooltip("体力が尽きたときに倒れるか 倒れない的はその場で満タンに戻る")]
    [SerializeField] private bool fallsOnDeath;

    [Tooltip("倒れてから戻るまでの秒数")]
    [Min(0f)]
    [SerializeField] private float reviveSeconds = 3f;

    [Tooltip("倒れている間に登録を外すダメージ対象")]
    [SerializeField] private DamageTargetBinding binding;

    [Tooltip("倒れている間に切る当たり判定")]
    [SerializeField] private Collider[] bodyColliders;

    [Tooltip("倒れている間に隠す見た目")]
    [SerializeField] private Renderer[] bodyRenderers;

    private float reviveTimer;

    public TeamType TeamType => TeamType.Enemy;

    /// <summary>
    /// 体力を管理するモデル
    /// </summary>
    public Health Health { get; private set; }
    /// <summary>初回生成時に必要な参照と状態を初期化する</summary>

    private void Awake()
    {
        ResetHealth();
    }

    private void Update()
    {
        if (reviveTimer <= 0f)
            return;

        reviveTimer -= Time.deltaTime;
        if (reviveTimer <= 0f)
            Revive();
    }

    /// <summary>
    /// ダメージを受ける処理
    /// </summary>
    /// <param name="damage">受けるダメージ量</param>
    /// <param name="knockbackForce">受けるノックバック力</param>
    public void TakeDamage(int damage, float knockbackForce)
    {
        if (Health.IsDead)
            return;

        DamageEffectManager.Instance?.ShowDamage(damage, transform.position);
        Health.TakeDamage(damage);
    }

    /// <summary>
    /// 死亡したときに呼び出される処理
    /// </summary>
    private void HandleDeath()
    {
        if (!fallsOnDeath)
        {
            ResetHealth();
            return;
        }

        SetBodyActive(false);
        reviveTimer = Mathf.Max(reviveSeconds, Mathf.Epsilon);
    }

    private void Revive()
    {
        ResetHealth();
        SetBodyActive(true);
    }

    private void ResetHealth()
    {
        Health = new Health(initialHP);
        Health.OnDeath += HandleDeath;
    }

    private void SetBodyActive(bool active)
    {
        foreach (Collider bodyCollider in bodyColliders)
            bodyCollider.enabled = active;
        foreach (Renderer bodyRenderer in bodyRenderers)
            bodyRenderer.enabled = active;

        if (active)
            binding.Register();
        else
            binding.Unregister();
    }
}
