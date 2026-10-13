using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

/// <summary>初回生成時の部品参照を保持し検索せず個体を取得と返却する</summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    private readonly Dictionary<GameObject, ObjectPool<PooledInstance>> pools = new();
    private readonly Dictionary<GameObject, PooledInstance> instances = new();

    /// <summary>シーンのプール管理を公開する</summary>
    private void Awake() => Instance = this;

    /// <summary>既存の呼び出しへGameObjectを返す</summary>
    public GameObject Get(GameObject prefab) => Rent(prefab).GameObject;

    /// <summary>保持済みの部品参照を持つ個体を取得する</summary>
    public PooledInstance Rent(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out var pool))
        {
            pool = CreatePool(prefab);
            pools.Add(prefab, pool);
        }
        return pool.Get();
    }

    /// <summary>生成済みの個体情報を登録表から返す</summary>
    public bool TryResolveInstance(GameObject instance, out PooledInstance cached) =>
        instances.TryGetValue(instance, out cached);

    /// <summary>Prefabごとの初回生成と保持済み参照の再利用を定義する</summary>
    private ObjectPool<PooledInstance> CreatePool(GameObject prefab)
    {
        ObjectPool<PooledInstance> pool = null;
        // Prefabの設定はこのプールの初期化時に一度だけ読み込む
        PoolableObject prefabPoolable = prefab.GetComponent<PoolableObject>();
        bool keepActive = prefabPoolable != null && prefabPoolable.KeepActiveInPool;
        pool = new ObjectPool<PooledInstance>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab);
                var cached = new PooledInstance(obj);
                instances.Add(obj, cached);
                // 返却先は初回生成時に関連付け、実行中は個体の登録表を引き直さない
                cached.Poolable?.SetPoolReturn(() => pool.Release(cached));
                return cached;
            },
            actionOnGet: cached =>
            {
                if (!keepActive) cached.GameObject.SetActive(true);
                cached.Lifecycle?.OnGet();
                cached.DamageBinding?.Register();
            },
            actionOnRelease: cached =>
            {
                cached.DamageBinding?.Unregister();
                cached.Lifecycle?.OnRelease();
                if (!keepActive) cached.GameObject.SetActive(false);
            },
            actionOnDestroy: cached =>
            {
                instances.Remove(cached.GameObject);
                Destroy(cached.GameObject);
            },
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 10000);
        return pool;
    }
}
