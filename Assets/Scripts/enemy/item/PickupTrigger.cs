using UnityEngine;
/// <summary>接触したプレイヤーへの取得効果と配置を管理する</summary>

public abstract class PickupTriggerItem : PoolableObject
{
    [Tooltip("床に置くときの傾きの最大角度 XとZの軸で指定する")]
    [SerializeField, Range(0f, 90f)] private float maxTilt = 15f;

    [Tooltip("床に埋める深さの最小値 アイテムの高さに対する割合")]
    [SerializeField, Range(0f, 1f)] private float minSink = 0f;

    [Tooltip("床に埋める深さの最大値 アイテムの高さに対する割合")]
    [SerializeField, Range(0f, 1f)] private float maxSink = 0f;

    private const float GroundRayLift = 1f;
    private const float GroundRayDistance = 100f;
    private const float GroundRaySkin = 0.01f;

    private static int groundMask;

    private bool pickedUp = false;
    private Renderer[] renderers;

    internal Rigidbody Rb { get; private set; }
    internal Transform CachedTransform { get; private set; }
    internal int MagnetIndex { get; set; } = -1;
    internal bool Captured { get; set; }
    /// <summary>初回生成時に必要な参照と状態を初期化する</summary>

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        CachedTransform = transform;
        renderers = GetComponentsInChildren<Renderer>();

        if (groundMask == 0)
            groundMask = ~LayerMask.GetMask("Enemy", "Pickup");
    }
    /// <summary>取得アイテムを地面の位置と傾きに合わせて配置する</summary>

    public void PlaceOnGround(Vector3 position, Transform dropper = null)
    {
        float yaw = Random.Range(0f, 360f);
        Quaternion rotation = Quaternion.Euler(
            Random.Range(-maxTilt, maxTilt),
            yaw,
            Random.Range(-maxTilt, maxTilt));

        if (renderers.Length == 0 || !FindGround(position, dropper, out RaycastHit hit))
        {
            CachedTransform.SetPositionAndRotation(position, rotation);
            return;
        }

        CachedTransform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

        float bottom = float.MaxValue;
        float top = float.MinValue;
        foreach (Renderer r in renderers)
        {
            Bounds bounds = r.bounds;
            bottom = Mathf.Min(bottom, bounds.min.y);
            top = Mathf.Max(top, bounds.max.y);
        }

        float sink = (top - bottom) * Random.Range(minSink, maxSink);
        position.y += hit.point.y - bottom - sink;
        CachedTransform.SetPositionAndRotation(position, rotation);
    }
    /// <summary>配置位置の下にある地面を物理判定で確認する</summary>

    private static bool FindGround(Vector3 position, Transform dropper, out RaycastHit hit)
    {
        Vector3 origin = position + Vector3.up * GroundRayLift;
        if (!Physics.Raycast(origin, Vector3.down, out hit, GroundRayDistance, groundMask, QueryTriggerInteraction.Ignore))
            return false;

        if (dropper == null || !hit.collider.transform.IsChildOf(dropper))
            return true;

        Vector3 below = hit.point + Vector3.down * GroundRaySkin;
        float remaining = GroundRayDistance - hit.distance - GroundRaySkin;
        return Physics.Raycast(below, Vector3.down, out hit, remaining, groundMask, QueryTriggerInteraction.Ignore);
    }
    /// <summary>有効化された個体を更新対象へ登録する</summary>

    private void OnEnable()
    {
        PickupMagnet.Register(this);
    }
    /// <summary>無効化された個体の登録と一時状態を解除する</summary>

    private void OnDisable()
    {
        PickupMagnet.Unregister(this);
    }
    /// <summary>新しい活動期間の状態を準備する</summary>


    
    public override void OnGet()
    {
        pickedUp = false;
        Captured = false;
    }
    /// <summary>侵入したColliderとの接触処理を開始する</summary>

    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp)
            return;

        if (!DamageTargetRegistry.TryResolve(other, out IDamageable pickupTarget) || !(pickupTarget is Player player))
            return;

        pickedUp = true;

        // アイテム固有の取得効果を適用する
        OnPickup(player);
      

        Release();
    }
    /// <summary>接触したプレイヤーへアイテムの取得効果を適用する</summary>
    
    protected abstract void OnPickup(Player player);
}