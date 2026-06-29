using UnityEngine;

public class TrackingProjectile : ProjectileObject
{
    [SerializeField] private float trackingRange = 15f;
    [SerializeField] private float rotateSpeed = 360f;
    [SerializeField] private float moveSpeed = 20f;

    private Transform target;

    private void FixedUpdate()
    {
        SearchTarget();

        if (target != null)
        {
            // 現在の進行方向
            Vector3 currentDir = rb.linearVelocity.normalized;

            // ターゲット方向
            Vector3 targetDir = (target.position - transform.position).normalized;

            // 少しずつ方向転換
            Vector3 newDir = Vector3.RotateTowards(currentDir,
                targetDir,
                rotateSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime,
                0f);

            rb.linearVelocity = newDir * moveSpeed;

            if (newDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(newDir);
            }
        }
        else
        {
            // 速度が落ちないよう維持
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
    }

    private void SearchTarget()
    {
        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);

            if (dist <= trackingRange)
                return;
        }

        target = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, trackingRange);

        float nearest = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (!hit.TryGetComponent(out IDamageable damageable))
                continue;

            if ((damageable.TeamType & targetTeam) == 0)
                continue;

            float d = (hit.transform.position - transform.position).sqrMagnitude;

            if (d < nearest)
            {
                nearest = d;
                target = hit.transform;
            }
        }
    }

    protected override void HandleHit(Collider collider)
    {
        if (collider.TryGetComponent(out IDamageable damageable))
        {
            if ((damageable.TeamType & targetTeam) == 0)
                return;

            damageable.TakeDamage(damage, knockbackForce);

            if (damageable.TeamType != TeamType.EnemyAmmo)
            {
                hasHit = true;
                Release();
            }
        }
        else
        {
            hasHit = true;
            Release();
        }
    }

    public override void OnGet()
    {
        base.OnGet();
        target = null;
    }

    public override void OnRelease()
    {
        target = null;
        base.OnRelease();
    }
}