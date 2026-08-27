using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/BulletData")]
public class BulletData : ScriptableObject
{
    [Min(0f)] public float Speed = 10f;
    [Min(0f)] public float Lifetime = 2f;
    public GameObject AmmpPrefab;

    public GameObject EffectPrefab;

    public GameObject Spawn(Transform shotPoint, Vector3 direction)
    {
        GameObject bullet = PoolManager.Instance.Get(AmmpPrefab);
        bullet.transform.SetPositionAndRotation(
            shotPoint.position,
            Quaternion.LookRotation(direction));

        if (EffectPrefab != null) {
            GameObject effect = PoolManager.Instance.Get(EffectPrefab);
            effect.transform.SetPositionAndRotation(
                shotPoint.position,
                Quaternion.LookRotation(direction));
        }

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = direction * Speed;
        }

        return bullet;
    }
}
