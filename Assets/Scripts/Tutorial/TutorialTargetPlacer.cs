using System.Collections.Generic;
using UnityEngine;

public class TutorialTargetPlacer : MonoBehaviour
{
    private const float GroundClearance = 0.05f;

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

    [Header("ランダム")]
    [Tooltip("ランダムの的の雛形 非アクティブのまま置き、ランダムにしたときに複製します")]
    [SerializeField] private TrainingDummy randomTargetTemplate;

    [Tooltip("ランダムにしたときに同時に出す的の数")]
    [Min(0)]
    [SerializeField] private int randomCount = 15;

    [Tooltip("ランダムの的を出す、プレイヤーからの最短距離")]
    [Min(0f)]
    [SerializeField] private float randomMinDistance = 10f;

    [Tooltip("ランダムの的を出す、プレイヤーからの最長距離")]
    [Min(0f)]
    [SerializeField] private float randomMaxDistance = 30f;

    [Tooltip("ランダムの的の場所を探す回数 地面が無い・物と重なる場所は引き直します")]
    [Min(1)]
    [SerializeField] private int randomPlacementAttempts = 10;

    [Tooltip("物との重なりを調べるときの的の半径")]
    [Min(0f)]
    [SerializeField] private float targetRadius = 0.5f;

    private readonly RaycastHit[] groundHits = new RaycastHit[16];
    private readonly Collider[] overlapHits = new Collider[16];
    private readonly List<TrainingDummy> randomTargets = new List<TrainingDummy>();
    private Transform player;
    private bool random;

    private void Update()
    {
        PlayerController found = FindFirstObjectByType<PlayerController>();
        if (found == null)
            return;

        player = found.transform;
        Place();
        ApplyMode();
        enabled = false;
    }

    public void SetRandom(bool value)
    {
        random = value;
        if (player != null)
            ApplyMode();
    }

    private void Place()
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

    private void ApplyMode()
    {
        foreach (TrainingDummy target in targets)
            target.gameObject.SetActive(!random);

        if (random)
        {
            ShowRandomTargets();
            return;
        }

        foreach (TrainingDummy target in randomTargets)
            target.gameObject.SetActive(false);
    }

    private void ShowRandomTargets()
    {
        if (randomTargetTemplate == null)
        {
            Debug.LogWarning("ランダムの的の雛形が無いため、ランダムの的を出せません", this);
            return;
        }

        while (randomTargets.Count < randomCount)
        {
            TrainingDummy target = Instantiate(randomTargetTemplate, randomTargetTemplate.transform.parent);
            target.name = $"{randomTargetTemplate.name}_{randomTargets.Count}";
            target.Reviving += MoveToRandomPosition;
            randomTargets.Add(target);
        }

        for (int i = 0; i < randomTargets.Count; i++)
        {
            TrainingDummy target = randomTargets[i];
            if (i >= randomCount)
            {
                target.gameObject.SetActive(false);
                continue;
            }

            MoveToRandomPosition(target);
            target.gameObject.SetActive(true);
        }
    }

    private void MoveToRandomPosition(TrainingDummy target)
    {
        if (player == null)
            return;

        float minDistance = Mathf.Min(randomMinDistance, randomMaxDistance);
        float maxDistance = Mathf.Max(randomMinDistance, randomMaxDistance);
        bool hasFallback = false;
        Vector3 fallback = default;

        for (int i = 0; i < randomPlacementAttempts; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(minDistance, maxDistance);
            Vector3 position = player.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
            if (!TryFindGround(position, player.position.y, out float groundY))
                continue;

            position.y = groundY + groundOffset;
            if (!IsBlocked(position, target))
            {
                SetTargetPosition(target, position);
                return;
            }

            hasFallback = true;
            fallback = position;
        }

        if (hasFallback)
            SetTargetPosition(target, fallback);
    }

    private static void SetTargetPosition(TrainingDummy target, Vector3 position)
    {
        target.transform.position = position;
        Physics.SyncTransforms();
    }

    private bool IsBlocked(Vector3 center, TrainingDummy self)
    {
        float groundY = center.y - groundOffset;
        Vector3 bottom = new Vector3(center.x, groundY + targetRadius + GroundClearance, center.z);
        Vector3 top = new Vector3(center.x, Mathf.Max(bottom.y, groundY + groundOffset * 2f - targetRadius), center.z);
        int count = Physics.OverlapCapsuleNonAlloc(bottom, top, targetRadius, overlapHits,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (overlapHits[i].GetComponentInParent<TrainingDummy>() != self)
                return true;
        }
        return false;
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
