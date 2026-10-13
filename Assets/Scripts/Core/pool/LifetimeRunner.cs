using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>登録された個体の有効期間をまとめて更新する</summary>
[DisallowMultipleComponent, DefaultExecutionOrder(-10000)]
public class LifetimeRunner : MonoBehaviour
{
    /// <summary>Sceneに配置した更新担当を通常の個体より先に公開する</summary>
    private void Awake()
    {
        if (instance != null && instance != this)
            throw new InvalidOperationException("Sceneの更新担当が重複しています: LifetimeRunner");
        instance = this;
    }

    /// <summary>Scene終了時に登録番号と静的参照を解放する</summary>
    private void OnDestroy()
    {
        if (instance != this) return;
        // 破棄順にかかわらず、個体側の無効化処理が古い登録番号を使用しないようにする
        foreach (var entry in owners) entry.LifetimeIndex = -1;
        owners.Clear();
        expireTimes.Clear();
        instance = null;
    }

    private static LifetimeRunner instance;
    private static readonly List<ILifetimeOwner> owners = new List<ILifetimeOwner>();
    private static readonly List<float> expireTimes = new List<float>();

    /// <summary>プレイ開始時に前回の静的な登録情報を初期化する</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        owners.Clear();
        expireTimes.Clear();
    }

    /// <summary>個体をSceneの更新担当へ登録する</summary>
    public static void Register(ILifetimeOwner owner, float duration)
    {
        if (instance == null)
            throw new InvalidOperationException("SceneにLifetimeRunnerを事前配置してください");

        float expireTime = Time.time + duration;

        int index = owner.LifetimeIndex;
        if (index >= 0)
        {
            expireTimes[index] = expireTime;
            return;
        }

        owner.LifetimeIndex = owners.Count;
        owners.Add(owner);
        expireTimes.Add(expireTime);
    }

    /// <summary>個体を登録から外して末尾の個体の番号を更新する</summary>
    public static void Unregister(ILifetimeOwner owner)
    {
        int index = owner.LifetimeIndex;
        if (index < 0)
            return;

        int last = owners.Count - 1;
        ILifetimeOwner moved = owners[last];
        owners[index] = moved;
        expireTimes[index] = expireTimes[last];
        moved.LifetimeIndex = index;
        owners.RemoveAt(last);
        expireTimes.RemoveAt(last);
        owner.LifetimeIndex = -1;
    }

    /// <summary>有効な登録対象をまとめて更新する</summary>
    private void Update()
    {
        float now = Time.time;

        for (int i = owners.Count - 1; i >= 0; i--)
        {
            if (i >= owners.Count)
                continue;

            if (expireTimes[i] > now)
                continue;

            ILifetimeOwner owner = owners[i];
            Unregister(owner);

            try
            {
                owner.OnLifetimeExpired();
            }
            catch (Exception e)
            {
                Debug.LogException(e, owner as UnityEngine.Object);
            }
        }
    }
}
