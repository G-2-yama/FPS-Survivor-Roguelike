using UnityEngine;
/// <summary>プレイヤーへの命中時に範囲攻撃を生成する</summary>

public class ballprojectile : enemyplojectileobject
{
    /// <summary>保持済みの登録情報を使って対象への命中を処理する</summary>
    [SerializeField] private GameObject prefab;
    

    protected override void HandleHit(Collider collider)
    {
        if (DamageTargetRegistry.TryResolve(collider, out IDamageable damageable))
        {
            if ((damageable.TeamType & targetTeam) == 0)
            {
                return;
            }
            hasHit = true;
            GameObject area = PoolManager.Instance.Get(prefab);
            area.transform.position = transform.position;
            Sounder?.Play(SoundCategory.Enemy);
            Release();
        }
    }
}
