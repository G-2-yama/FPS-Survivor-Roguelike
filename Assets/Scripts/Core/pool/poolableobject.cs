using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class PoolableObject : MonoBehaviour, IPoolable
{
    ObjectPool<GameObject> pool;
   

    public void SetPool(ObjectPool<GameObject> pool)
    {
        this.pool = pool;
    }

    public void Release()
    {
        if (pool != null)
        {
            pool.Release(gameObject);
        }
    }

    public virtual void OnGet() { }
    public virtual void OnRelease() { }
}