using System;
using InfiniteTileWorld;
using UnityEngine;

public class LockdownController : MonoBehaviour
{
    public enum LockdownState
    {
        Idle,
        Warning,
        Closed,
        Opening,
    }

    private static readonly Vector3[] ExitDirections =
    {
        Vector3.forward,
        Vector3.back,
        Vector3.right,
        Vector3.left,
    };

    [Header("参照")]
    [Tooltip("経過時間を読むタイマー")]
    [SerializeField] private Timer timer;

    [Tooltip("広場の位置と地形の周期を求める地形の管理")]
    [SerializeField] private StageManager stageManager;

    [Tooltip("閉じ込める対象のプレイヤー")]
    [SerializeField] private Player player;

    [Tooltip("出口を塞ぐ扉のPrefab 厚みはこのBoxColliderのZの大きさを使う")]
    [SerializeField] private BoxCollider doorPrefab;

    [Tooltip("閉じ込めの予定表")]
    [SerializeField] private LockdownSchedule schedule;

    [Header("広場")]
    [Tooltip("広場の一辺のタイル数")]
    [SerializeField, Min(1)] private int plazaTiles = 3;

    [Tooltip("地面を探すときに当てるレイヤー")]
    [SerializeField] private LayerMask groundMask = 1;

    [Tooltip("地面を探すレイを、広場の中心の何m上から下へ飛ばすか")]
    [SerializeField, Min(0f)] private float groundProbeHeight = 100f;

    [Tooltip("道にいたプレイヤーを、出口から何m内側へ移すか")]
    [SerializeField, Min(0f)] private float teleportInset = 5f;

    [Header("扉の動き")]
    [Tooltip("警告の間、扉を閉じた位置より何m上に吊っておくか")]
    [SerializeField, Min(0f)] private float doorHangHeight = 25f;

    [Tooltip("閉まる何秒前から扉を下ろし始めるか")]
    [SerializeField, Min(0.01f)] private float doorLowerSeconds = 3f;

    [Tooltip("開くときに扉を上げきるまでの秒数")]
    [SerializeField, Min(0.01f)] private float doorRaiseSeconds = 2f;

    [Header("デバッグ")]
    [Tooltip("デバッグ画面から閉じたときに閉じている長さ（秒）")]
    [SerializeField, Min(0f)] private float debugCloseDuration = 20f;

    private readonly BoxCollider[] doors = new BoxCollider[4];
    private readonly Vector3[] doorClosedPositions = new Vector3[4];
    private CharacterController playerBody;
    private Vector3 plazaOrigin;
    private float plazaPeriod;
    private float plazaHalfExtent;
    private float doorThickness;
    private int nextEntryIndex;
    private float closeTime;
    private float openTime;
    private float raiseStartTime;

    public LockdownState State { get; private set; }
    public Vector3 PlazaCenter { get; private set; }
    public float SecondsUntilClose => State == LockdownState.Warning ? Mathf.Max(0f, closeTime - timer.ElapsedTime) : 0f;
    public float SecondsUntilOpen => State == LockdownState.Closed ? Mathf.Max(0f, openTime - timer.ElapsedTime) : 0f;
    public bool IsPlayerInside => IsInsidePlaza(playerBody.transform.position);

    public event Action<LockdownState> StateChanged;

    private void Start()
    {
        bool modeEnabled = GameStartSettings.Mode == null || GameStartSettings.Mode.LockdownEnabled;
        if (GameStartSettings.IsTutorial || !modeEnabled || schedule == null)
        {
            enabled = false;
            return;
        }

        int gridSize = stageManager.GridSize;
        int centerIndex = gridSize / 2 * gridSize + gridSize / 2;
        plazaOrigin = stageManager.Panels[centerIndex].WorldCenter;
        plazaPeriod = stageManager.TileSize * gridSize;
        plazaHalfExtent = stageManager.TileSize * plazaTiles * 0.5f;
        doorThickness = doorPrefab.size.z * doorPrefab.transform.localScale.z;
        playerBody = player.GetComponent<CharacterController>();

        for (int i = 0; i < doors.Length; i++)
        {
            doors[i] = Instantiate(doorPrefab, transform);
            doors[i].gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        float now = timer.ElapsedTime;

        switch (State)
        {
            case LockdownState.Closed:
                if (now >= openTime) BeginOpening();
                return;

            case LockdownState.Opening:
                float raised = Mathf.Clamp01((now - raiseStartTime) / doorRaiseSeconds);
                SetDoorHeight(raised * doorHangHeight);
                if (raised >= 1f) FinishOpening();
                return;
        }

        var entries = schedule.Entries;
        if (nextEntryIndex >= entries.Count) return;

        LockdownEntry entry = entries[nextEntryIndex];
        if (now >= entry.CloseTime)
        {
            nextEntryIndex++;
            if (State != LockdownState.Warning) PrepareDoors();
            Close(entry.OpenTime);
        }
        else if (now >= entry.WarningStartTime)
        {
            if (State != LockdownState.Warning)
            {
                PrepareDoors();
                closeTime = entry.CloseTime;
                ChangeState(LockdownState.Warning);
            }

            float remaining = Mathf.Clamp01((closeTime - now) / doorLowerSeconds);
            SetDoorHeight(remaining * doorHangHeight);
        }
    }

    private void ChangeState(LockdownState next)
    {
        State = next;
        StateChanged?.Invoke(next);
    }

    private void PrepareDoors()
    {
        PlazaCenter = NearestPlazaCenter(playerBody.transform.position);
        PlaceDoors(PlazaCenter);
        SetDoorHeight(doorHangHeight);
    }

    private void Close(float openAt)
    {
        SetDoorHeight(0f);
        MovePlayerInside(PlazaCenter);
        foreach (BoxCollider door in doors) door.enabled = true;
        openTime = openAt;
        ChangeState(LockdownState.Closed);
    }

    private void BeginOpening()
    {
        foreach (BoxCollider door in doors) door.enabled = false;
        raiseStartTime = timer.ElapsedTime;
        ChangeState(LockdownState.Opening);
    }

    private void FinishOpening()
    {
        foreach (BoxCollider door in doors) door.gameObject.SetActive(false);
        ChangeState(LockdownState.Idle);

        var entries = schedule.Entries;
        float now = timer.ElapsedTime;
        while (nextEntryIndex < entries.Count && entries[nextEntryIndex].CloseTime < now)
        {
            nextEntryIndex++;
        }
    }

    private Vector3 NearestPlazaCenter(Vector3 position)
    {
        float x = plazaOrigin.x + Mathf.Round((position.x - plazaOrigin.x) / plazaPeriod) * plazaPeriod;
        float z = plazaOrigin.z + Mathf.Round((position.z - plazaOrigin.z) / plazaPeriod) * plazaPeriod;
        return new Vector3(x, plazaOrigin.y, z);
    }

    private void PlaceDoors(Vector3 center)
    {
        float halfTile = stageManager.TileSize * 0.5f;

        for (int i = 0; i < ExitDirections.Length; i++)
        {
            Vector3 direction = ExitDirections[i];
            Vector3 edge = center + direction * plazaHalfExtent;
            float insideGround = FindGroundY(edge - direction * halfTile, center.y);
            float outsideGround = FindGroundY(edge + direction * halfTile, center.y);

            Vector3 position = edge + direction * (doorThickness * 0.5f);
            position.y = Mathf.Min(insideGround, outsideGround);
            doorClosedPositions[i] = position;

            doors[i].enabled = false;
            doors[i].transform.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            doors[i].gameObject.SetActive(true);
        }
    }

    private void SetDoorHeight(float height)
    {
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].transform.position = doorClosedPositions[i] + Vector3.up * height;
        }
    }

    private bool IsInsidePlaza(Vector3 position)
    {
        float limit = plazaHalfExtent - playerBody.radius;
        return Mathf.Abs(position.x - PlazaCenter.x) <= limit && Mathf.Abs(position.z - PlazaCenter.z) <= limit;
    }

    private void MovePlayerInside(Vector3 center)
    {
        Vector3 position = playerBody.transform.position;
        if (IsInsidePlaza(position)) return;

        float offsetX = position.x - center.x;
        float offsetZ = position.z - center.z;
        Vector3 exitDirection = Mathf.Abs(offsetX) >= Mathf.Abs(offsetZ)
            ? new Vector3(Mathf.Sign(offsetX), 0f, 0f)
            : new Vector3(0f, 0f, Mathf.Sign(offsetZ));
        Vector3 target = center + exitDirection * (plazaHalfExtent - teleportInset);

        float footOffset = position.y - player.FootPosition.y;
        target.y = FindGroundY(target, position.y - footOffset) + footOffset + playerBody.skinWidth;

        playerBody.enabled = false;
        playerBody.transform.position = target;
        playerBody.enabled = true;
        Physics.SyncTransforms();
    }

    private float FindGroundY(Vector3 point, float fallback)
    {
        Vector3 origin = new Vector3(point.x, plazaOrigin.y + groundProbeHeight, point.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundProbeHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point.y;
        }
        return fallback;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugClose()
    {
        if (!enabled || State == LockdownState.Closed || State == LockdownState.Opening) return;
        if (State != LockdownState.Warning) PrepareDoors();
        Close(timer.ElapsedTime + debugCloseDuration);
    }

    public void DebugOpen()
    {
        if (!enabled || State != LockdownState.Closed) return;
        BeginOpening();
    }
#endif
}
