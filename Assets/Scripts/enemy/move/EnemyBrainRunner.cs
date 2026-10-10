using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBrainRunner : MonoBehaviour
{
    private static EnemyBrainRunner instance;
    private static readonly List<EnemyBrain> brains = new List<EnemyBrain>();

    private static Transform cachedTarget;
    private static Vector3 cachedTargetPosition;

    private EnemyBrain[] snapshot = Array.Empty<EnemyBrain>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        brains.Clear();
        cachedTarget = null;
    }

    public static Vector3 GetTargetPosition(Transform target)
    {
        if (!ReferenceEquals(target, cachedTarget))
        {
            cachedTarget = target;
            cachedTargetPosition = target.position;
        }
        return cachedTargetPosition;
    }

    public static void Register(EnemyBrain brain)
    {
        if (instance == null)
        {
            instance = new GameObject(nameof(EnemyBrainRunner)).AddComponent<EnemyBrainRunner>();
            DontDestroyOnLoad(instance.gameObject);
        }

        brain.RunnerIndex = brains.Count;
        brains.Add(brain);
    }

    public static void Unregister(EnemyBrain brain)
    {
        int index = brain.RunnerIndex;
        if (index < 0)
            return;

        int last = brains.Count - 1;
        EnemyBrain moved = brains[last];
        brains[index] = moved;
        moved.RunnerIndex = index;
        brains.RemoveAt(last);
        brain.RunnerIndex = -1;
    }

    private void FixedUpdate()
    {
        int count = brains.Count;
        if (snapshot.Length < count)
        {
            snapshot = new EnemyBrain[Mathf.NextPowerOfTwo(count)];
        }
        brains.CopyTo(snapshot);
        cachedTarget = null;

        for (int i = 0; i < count; i++)
        {
            EnemyBrain brain = snapshot[i];
            if (brain.RunnerIndex < 0)
                continue;

            try
            {
                brain.Tick();
            }
            catch (Exception e)
            {
                Debug.LogException(e, brain);
            }
        }

        Array.Clear(snapshot, 0, count);
        cachedTarget = null;
    }
}
