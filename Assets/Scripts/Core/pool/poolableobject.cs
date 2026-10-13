using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>初回生成時に関連付けた返却先と個体の活動期間を管理する</summary>
public class PoolableObject : MonoBehaviour, IPoolable
{
    private Action returnToPool;
   

    /// <summary>従来のGameObjectプールを使う呼び出しへ返却先を提供する</summary>
    public void SetPool(ObjectPool<GameObject> pool)
    {
        returnToPool = pool != null ? () => pool.Release(gameObject) : null;
    }

    /// <summary>参照保持済みの個体を直接返す処理を初回生成時に関連付ける</summary>
    public void SetPoolReturn(Action returnAction) => returnToPool = returnAction;

    /// <summary>個体の登録表を探索せず関連付けた返却先へ戻す</summary>
    public void Release()
    {
        returnToPool?.Invoke();
    }

    public virtual bool KeepActiveInPool => false;

    /// <summary>プールから取得された個体の活動期間を開始する</summary>
    public virtual void OnGet() { }
    /// <summary>プールへ返す個体の活動期間を終了する</summary>
    public virtual void OnRelease() { }
}
