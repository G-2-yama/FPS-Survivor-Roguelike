using UnityEngine;

public class TutorialTargetPlacer : MonoBehaviour
{
    [Tooltip("プレイヤーの開始位置からの相対で置く的 子の位置はプレイヤーの向きを前としたときの位置")]
    [SerializeField] private TrainingDummy[] targets;

    [Tooltip("的の足元から中心までの高さ")]
    [Min(0f)]
    [SerializeField] private float groundOffset = 1f;

    [Tooltip("地面を探すレイをプレイヤーの高さからどれだけ上から撃つか")]
    [Min(0f)]
    [SerializeField] private float groundSearchHeight = 10f;

    [Tooltip("地面を探すレイの長さ")]
    [Min(0f)]
    [SerializeField] private float groundSearchDistance = 80f;

    private readonly RaycastHit[] groundHits = new RaycastHit[16];
    private bool placed;

    private void Update()
    {
        if (placed)
            return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null)
            return;

        Place(player.transform);
        placed = true;
        enabled = false;
    }

    private void Place(Transform player)
    {
        Vector3 forward = Vector3.ProjectOnPlane(player.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;

        transform.SetPositionAndRotation(player.position, Quaternion.LookRotation(forward.normalized, Vector3.up));

        foreach (TrainingDummy target in targets)
        {
            Vector3 position = target.transform.position;
            if (TryFindGround(position, player.position.y, out float groundY))
                position.y = groundY + groundOffset;
            target.transform.position = position;
        }

        Physics.SyncTransforms();
    }

    private bool TryFindGround(Vector3 position, float playerY, out float groundY)
    {
        Vector3 origin = new Vector3(position.x, playerY + groundSearchHeight, position.z);
        int count = Physics.RaycastNonAlloc(origin, Vector3.down, groundHits, groundSearchDistance,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        groundY = 0f;
        float nearest = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = groundHits[i];
            if (hit.distance >= nearest || hit.collider.GetComponentInParent<TrainingDummy>() != null
                || hit.collider.GetComponentInParent<PlayerController>() != null)
                continue;

            nearest = hit.distance;
            groundY = hit.point.y;
        }
        return nearest < float.MaxValue;
    }
}
