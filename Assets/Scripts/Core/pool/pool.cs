using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;

    Dictionary<GameObject, ObjectPool<GameObject>> pools =
        new Dictionary<GameObject, ObjectPool<GameObject>>();

    void Awake()
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

    ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        return new ObjectPool<GameObject>(
            () =>
            {
                var obj = Instantiate(prefab);

                var poolable = obj.GetComponent<PoolableObject>();
                if (poolable != null)
                {
                    poolable.SetPool(pools[prefab]);
                }

                return obj;
            },

            obj =>
            {
                obj.SetActive(true);

                var poolable = obj.GetComponent<IPoolable>();
                poolable?.OnGet();
            },

            obj =>
            {
                var poolable = obj.GetComponent<IPoolable>();
                poolable?.OnRelease();

                obj.SetActive(false);
            },

            obj => Destroy(obj),
            true,
            10,
            100
        );
    }
}