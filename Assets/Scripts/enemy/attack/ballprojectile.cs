using UnityEngine;
/// <summary>プレイヤーへの命中時に範囲攻撃を生成する</summary>

public class ballprojectile : enemyplojectileobject
{
    /// <summary>保持済みの登録情報を使って対象への命中を処理する</summary>
    [SerializeField] private GameObject prefab;
    private DamageBase explosionSource;

    private void Awake()
    {
        explosionSource = prefab != null ? prefab.GetComponent<DamageBase>() : null;
    }

    protected override void HandleHit(Collider collider)
    {
        if (DamageTargetRegistry.TryResolve(collider, out IDamageable damageable))
        {
            if ((damageable.TeamType & targetTeam) == 0)
            {
                return;
            }
            hasHit = true;
            PooledInstance area = PoolManager.Instance.Rent(prefab);
            area.GameObject.transform.position = transform.position;
            if (explosionSource != null)
            {
                area.RequireDamage().Initialize(
                    EnemyStatScaling.Scale(explosionSource.Damage, attackMultiplier),
                    explosionSource.KnockbackForce);
            }
            Sounder?.Play(SoundCategory.Enemy);
            Release();
        }
    }
}
