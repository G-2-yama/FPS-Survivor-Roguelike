using UnityEngine;
/// <summary>有効な登録済み対象を追尾して攻撃個体を移動する</summary>

public class TrackingMovement : MovementBase
{
    [SerializeField] private float trackingRange = 15f;
    [SerializeField] private float rotateSpeed = 360f;
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float searchInterval = 0.1f;

    private Transform target;
    private Collider targetCollider;
    private float searchTimer;
    private DamageBase damageBase;
    /// <summary>初回生成時に必要な参照と状態を初期化する</summary>

    private void Awake()
    {
        damageBase = GetComponent<DamageBase>();
    }

    /// <summary>再利用する弾の前回の追尾対象を破棄する</summary>
    public override void Initialize(Transform owner, Vector3 direction)
    {
        base.Initialize(owner, direction);
        target = null;
        targetCollider = null;
        searchTimer = 0f;
    }
    /// <summary>保持済みの参照を使って個体の状態を更新する</summary>

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
    /// <summary>有効な追尾対象を維持し失った場合だけ候補を選び直す</summary>

    private void SearchTarget()
    {
        // 既にターゲットがいて範囲内ならそのまま
        if (target != null)
        {
            float sqrDist = (target.position - transform.position).sqrMagnitude;

            if (DamageTargetRegistry.TryResolve(targetCollider, out _) && sqrDist <= trackingRange * trackingRange)
                return;

            target = null;
            targetCollider = null;
            searchTimer = 0f;
        }

        searchTimer -= Time.deltaTime;
        if (searchTimer > 0f)
            return;

        searchTimer = searchInterval;

        Collider nearest = DamageTargetSearch.FindNearest(transform.position, trackingRange, damageBase.TargetTeam, out _);
        if (nearest != null)
        {
            target = nearest.transform;
            targetCollider = nearest;
        }
    }
}