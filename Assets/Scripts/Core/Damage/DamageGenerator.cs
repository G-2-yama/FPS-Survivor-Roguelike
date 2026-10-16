using UnityEngine;
using System.Collections.Generic;
/// <summary>命中時に爆発個体を生成してダメージを引き継ぐ</summary>

public class DamageGenerator : DamageBase
{
    [SerializeField] private List<GameObject> explosionPrefabs;
    [SerializeField] private Sounder sounder;
    private float areaExtraLifetime;
    private int areaDamagePerSecond;

    public float AreaLifetime
    {
        get
        {
            float result = 0f;
            if (explosionPrefabs == null)
                return result;
            foreach (GameObject prefab in explosionPrefabs)
            {
                if (prefab != null && prefab.TryGetComponent(out DamageArea area))
                    result = Mathf.Max(result, area.Lifetime);
            }
            return result;
        }
    }

    /// <summary>取得した個体へ今回の攻撃設定を渡す</summary>
    public override void Initialize(int damage, float knockback)
    {
        base.Initialize(damage, knockback);
        // 爆発の値は生成した個体へ渡し共有Prefabを変更しない
    }

    public override void ApplyAreaModifiers(float extraLifetime, int damagePerSecond)
    {
        areaExtraLifetime = extraLifetime;
        areaDamagePerSecond = damagePerSecond;
    }

    public override void OnGet()
    {
        base.OnGet();
        areaExtraLifetime = 0f;
        areaDamagePerSecond = 0;
    }
    /// <summary>侵入したColliderとの接触処理を開始する</summary>

    private void OnTriggerEnter(Collider other)
    {
        if (isReleased)
            return;

        if (TryDamage(other, out _))
        {
            Explode();
        }
    }
    /// <summary>物理衝突したColliderへの命中を処理する</summary>

    private void OnCollisionEnter(Collision collision)
    {
        if (isReleased)
            return;

        // 敵でも壁でも爆発
        if (TryDamage(collision.collider, out _))
        {
            Explode();
        }
        else
        {
            Explode();
        }
    }
    /// <summary>保持したダメージを生成した爆発個体へ渡す</summary>

    private void Explode()
    {
        sounder?.Play(SoundCategory.Generate);
        foreach (GameObject prefab in explosionPrefabs)
        {
            PooledInstance explosion = PoolManager.Instance.Rent(prefab);
            GameObject obj = explosion.GameObject;

            obj.transform.position = transform.position;

            DamageBase explosionDamage = explosion.RequireDamage();
            explosionDamage.Initialize(damage, knockbackForce);
            explosionDamage.ApplyAreaModifiers(areaExtraLifetime, areaDamagePerSecond);
        }

        Release();
    }
}
