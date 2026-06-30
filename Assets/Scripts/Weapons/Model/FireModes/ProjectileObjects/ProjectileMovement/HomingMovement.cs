using UnityEngine;

public class TrackingMovement : MovementBase
{
    [SerializeField] private float trackingRange = 15f;
    [SerializeField] private float rotateSpeed = 360f;
    [SerializeField] private float moveSpeed = 20f;

    private Transform target;
    private DamageBase damageBase;

    private void Awake()
    {
        damageBase = GetComponent<DamageBase>();
    }

    protected override void Update()
    {
        SearchTarget();

        if (target != null)
        {
            Vector3 targetDir = (target.position - transform.position).normalized;

            direction = Vector3.RotateTowards(
                direction,
                targetDir,
                rotateSpeed * Mathf.Deg2Rad * Time.deltaTime,
                0f);
        }

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void SearchTarget()
    {
        // 既にターゲットがいて範囲内ならそのまま
        if (target != null)
        {
            float sqrDist = (target.position - transform.position).sqrMagnitude;

            if (sqrDist <= trackingRange * trackingRange)
                return;
        }

        target = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, trackingRange);

        float nearest = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable damageable))
                continue;

            if ((damageable.TeamType & damageBase.TargetTeam) == 0)
                continue;

            float sqrDist = (hit.transform.position - transform.position).sqrMagnitude;

            if (sqrDist < nearest)
            {
                nearest = sqrDist;
                target = hit.transform;
            }
        }
    }
}