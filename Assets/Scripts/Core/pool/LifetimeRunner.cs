using System;
using System.Collections.Generic;
using UnityEngine;

public class LifetimeRunner : MonoBehaviour
{
    private static LifetimeRunner instance;
    private static readonly List<ILifetimeOwner> owners = new List<ILifetimeOwner>();
    private static readonly List<float> expireTimes = new List<float>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        owners.Clear();
        expireTimes.Clear();
    }

    public static void Register(ILifetimeOwner owner, float duration)
    {
        if (instance == null)
        {
            instance = new GameObject(nameof(LifetimeRunner)).AddComponent<LifetimeRunner>();
            DontDestroyOnLoad(instance.gameObject);
        }

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
