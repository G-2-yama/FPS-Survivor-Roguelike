using UnityEngine;
/// <summary>敵の弾と発射時のエフェクト設定を保持する</summary>

[CreateAssetMenu(menuName = "Enemy/BulletData")]
public class BulletData : ScriptableObject
{
    [Min(0f)] public float Speed = 10f;
    [Min(0f)] public float Lifetime = 2f;
    public GameObject AmmpPrefab;

    public GameObject EffectPrefab;
    /// <summary>既存の呼び出しへ発射した弾のGameObjectを返す</summary>

    public GameObject Spawn(Transform shotPoint, Vector3 direction) => SpawnInstance(shotPoint, direction).GameObject;

    /// <summary>初期化済みの部品参照を持つ弾を発射する</summary>
    public PooledInstance SpawnInstance(Transform shotPoint, Vector3 direction)
    {
        PooledInstance spawned = PoolManager.Instance.Rent(AmmpPrefab);
        GameObject bullet = spawned.GameObject;
        bullet.transform.SetPositionAndRotation(
            shotPoint.position,
            Quaternion.LookRotation(direction));

        if (EffectPrefab != null) {
            GameObject effect = PoolManager.Instance.Get(EffectPrefab);
            effect.transform.SetPositionAndRotation(
                shotPoint.position,
                Quaternion.LookRotation(direction));
        }

        Rigidbody rb = spawned.Body;
        if (rb != null)
        {
            rb.linearVelocity = direction * Speed;
        }

        return spawned;
    }
}
