using System;
using System.Collections.Generic;
using UnityEngine;

public class DamagePopupRunner : MonoBehaviour
{
    private static DamagePopupRunner instance;
    private static readonly List<DamagePopup> popups = new List<DamagePopup>();

    private DamagePopup[] snapshot = Array.Empty<DamagePopup>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        popups.Clear();
    }

    public static void Register(DamagePopup popup)
    {
        if (popup.RunnerIndex >= 0)
            return;

        if (instance == null)
        {
            instance = new GameObject(nameof(DamagePopupRunner)).AddComponent<DamagePopupRunner>();
            DontDestroyOnLoad(instance.gameObject);
        }

        popup.RunnerIndex = popups.Count;
        popups.Add(popup);
    }

    public static void Unregister(DamagePopup popup)
    {
        int index = popup.RunnerIndex;
        if (index < 0)
            return;

        int last = popups.Count - 1;
        DamagePopup moved = popups[last];
        popups[index] = moved;
        moved.RunnerIndex = index;
        popups.RemoveAt(last);
        popup.RunnerIndex = -1;
    }

    private void LateUpdate()
    {
        int count = popups.Count;
        if (snapshot.Length < count)
        {
            snapshot = new DamagePopup[Mathf.NextPowerOfTwo(count)];
        }
        popups.CopyTo(snapshot);

        for (int i = 0; i < count; i++)
        {
            DamagePopup popup = snapshot[i];
            if (popup.RunnerIndex < 0)
                continue;

            try
            {
                popup.Tick();
            }
            catch (Exception e)
            {
                Debug.LogException(e, popup);
            }
        }

        Array.Clear(snapshot, 0, count);
    }
}
