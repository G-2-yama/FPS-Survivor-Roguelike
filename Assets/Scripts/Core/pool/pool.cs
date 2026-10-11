using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    private Dictionary<GameObject, ObjectPool<GameObject>> pools
        = new Dictionary<GameObject, ObjectPool<GameObject>>();

    private void Awake()
    {
        Instance = this;
    }

    public GameObject Get(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = CreatePool(prefab);
        }

        return pools[prefab].Get();
    }
    

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        ObjectPool<GameObject> pool = null;

        PoolableObject prefabPoolable = prefab.GetComponent<PoolableObject>();
        bool keepActive = prefabPoolable != null && prefabPoolable.KeepActiveInPool;

        pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject obj = Instantiate(prefab);

                PoolableObject poolable =
                    obj.GetComponent<PoolableObject>();

                if (poolable != null)
                {
                    poolable.SetPool(pool);
                }

                return obj;
            },

            actionOnGet: obj =>
            {
                if (!keepActive)
                    obj.SetActive(true);

                IPoolable poolable =
                    obj.GetComponent<IPoolable>();

                poolable?.OnGet();
            },

            actionOnRelease: obj =>
            {
                IPoolable poolable =
                    obj.GetComponent<IPoolable>();

                poolable?.OnRelease();

                if (!keepActive)
                    obj.SetActive(false);
            },

            actionOnDestroy: obj =>
            {
                Destroy(obj);
            },

            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 10000
        );

        return pool;
    }
}