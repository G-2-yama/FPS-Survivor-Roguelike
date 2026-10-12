using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PickupMagnet : MonoBehaviour
{
    [Tooltip("引き寄せ中のアイテムが1秒に進む距離")]
    [SerializeField, Min(0f)] private float pullSpeed = 80f;

    [Tooltip("引き寄せ中のアイテムの高さの上限（ワールド座標の Y）")]
    [SerializeField] private float maxHeight = 2f;

    private const float ReleaseMargin = 1f;

    private static readonly List<PickupTriggerItem> items = new List<PickupTriggerItem>();

    private Player player;
    private Transform cachedTransform;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        items.Clear();
    }

    internal static void Register(PickupTriggerItem item)
    {
        if (item.MagnetIndex >= 0)
            return;

        item.MagnetIndex = items.Count;
        item.Captured = false;
        items.Add(item);
    }

    internal static void Unregister(PickupTriggerItem item)
    {
        int index = item.MagnetIndex;
        if (index < 0)
            return;

        int last = items.Count - 1;
        PickupTriggerItem moved = items[last];
        items[index] = moved;
        moved.MagnetIndex = index;
        items.RemoveAt(last);
        item.MagnetIndex = -1;
        item.Captured = false;
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        cachedTransform = transform;
    }

    private void FixedUpdate()
    {
        float radius = player.Stats.PickupRadius;
        float releaseRadius = radius + ReleaseMargin;
        Vector3 targetPosition = cachedTransform.position;

        for (int i = 0; i < items.Count; i++)
        {
            PickupTriggerItem item = items[i];
            float distance = Vector3.Distance(item.CachedTransform.position, targetPosition);

            if (!item.Captured)
            {
                if (distance <= radius)
                    item.Captured = true;
                continue;
            }

            if (distance > releaseRadius)
            {
                item.Captured = false;
                continue;
            }

            Pull(item.Rb, targetPosition);
        }
    }

    private void Pull(Rigidbody rb, Vector3 targetPosition)
    {
        Vector3 position = rb.position;
        Vector3 toTarget = targetPosition - position;

        if (toTarget.sqrMagnitude <= 0.0001f)
            return;

        Vector3 nextPos = position + toTarget.normalized * pullSpeed * Time.fixedDeltaTime;
        nextPos.y = Mathf.Min(nextPos.y, maxHeight);

        rb.MovePosition(nextPos);
        rb.MoveRotation(Quaternion.LookRotation(-toTarget));
    }
}
