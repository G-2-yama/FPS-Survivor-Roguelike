using System;
using System.Collections.Generic;
using UnityEngine;

public class BoostFireCullerRunner : MonoBehaviour
{
    private const float CheckInterval = 0.25f;

    private static BoostFireCullerRunner instance;
    private static readonly List<BoostFireCuller> cullers = new List<BoostFireCuller>();

    private int cursor;
    private float pending;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        cullers.Clear();
    }

    public static void Register(BoostFireCuller culler)
    {
        if (instance == null)
        {
            instance = new GameObject(nameof(BoostFireCullerRunner)).AddComponent<BoostFireCullerRunner>();
            DontDestroyOnLoad(instance.gameObject);
        }

        culler.RunnerIndex = cullers.Count;
        cullers.Add(culler);
    }

    public static void Unregister(BoostFireCuller culler)
    {
        int index = culler.RunnerIndex;
        if (index < 0)
            return;

        int last = cullers.Count - 1;
        BoostFireCuller moved = cullers[last];
        cullers[index] = moved;
        moved.RunnerIndex = index;
        cullers.RemoveAt(last);
        culler.RunnerIndex = -1;
    }

    private void Update()
    {
        int count = cullers.Count;
        if (count == 0)
            return;

        Camera camera = Camera.main;
        if (camera == null)
            return;

        Vector3 cameraPosition = camera.transform.position;

        pending = Mathf.Min(pending + count * Time.deltaTime / CheckInterval, count);
        int budget = Mathf.Min((int)pending, count);
        pending -= budget;

        for (int i = 0; i < budget; i++)
        {
            if (cursor >= cullers.Count)
                cursor = 0;

            BoostFireCuller culler = cullers[cursor];
            cursor++;

            try
            {
                culler.Refresh(cameraPosition);
            }
            catch (Exception e)
            {
                Debug.LogException(e, culler);
            }
        }
    }
}
