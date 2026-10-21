using UnityEngine;

/// <summary>箱型の生成物を地面に合わせ、本体全体が壁や他の物体に重ならない位置だけを採用する</summary>
public static class GroundedSpawnPlacement
{
    private const float GroundRayLift = 2f;
    private const float GroundRayDistance = 32f;
    private const float Clearance = 0.05f;
    private static readonly RaycastHit[] groundHits = new RaycastHit[32];
    private static readonly Collider[] overlaps = new Collider[32];

    public static bool TryGetPosition(GameObject source, Vector3 candidate, out Vector3 position)
    {
        position = default;
        if (source == null || !source.TryGetComponent(out BoxCollider box) || !box.enabled || box.isTrigger)
            return false;

        // 同じフレームに生成した箱や移動したステージも、次の配置判定に反映する。
        Physics.SyncTransforms();
        int groundMask = Physics.DefaultRaycastLayers & ~LayerMask.GetMask(
            "Enemy", "Pickup", "PlayerBody", "PlayerPushZone", "PlayerObstacle", "UI");
        int count = Physics.RaycastNonAlloc(candidate + Vector3.up * GroundRayLift, Vector3.down,
            groundHits, GroundRayDistance, groundMask, QueryTriggerInteraction.Ignore);
        if (count == groundHits.Length)
            return false;

        int nearest = -1;
        float distance = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            if (groundHits[i].collider.transform.IsChildOf(source.transform) || groundHits[i].distance >= distance)
                continue;
            nearest = i;
            distance = groundHits[i].distance;
        }
        if (nearest < 0 || groundHits[nearest].normal.y < 0.7f)
            return false;

        Vector3 scale = box.transform.lossyScale;
        scale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        Vector3 halfSize = Vector3.Scale(box.size, scale) * 0.5f;
        Quaternion rotation = box.transform.rotation;
        Vector3 offset = box.transform.TransformPoint(box.center) - source.transform.position;
        float verticalExtent = Mathf.Abs((rotation * Vector3.right).y) * halfSize.x
            + Mathf.Abs((rotation * Vector3.up).y) * halfSize.y
            + Mathf.Abs((rotation * Vector3.forward).y) * halfSize.z;

        position = candidate;
        position.y = groundHits[nearest].point.y + verticalExtent - offset.y + Clearance;
        count = Physics.OverlapBoxNonAlloc(position + offset, halfSize + Vector3.one * (Clearance * 0.5f),
            overlaps, rotation, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        if (count == overlaps.Length)
            return false;
        for (int i = 0; i < count; i++)
            if (!overlaps[i].transform.IsChildOf(source.transform))
                return false;

        return true;
    }
}
