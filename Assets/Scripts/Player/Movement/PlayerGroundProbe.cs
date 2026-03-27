using UnityEngine;

/// <summary>
/// プレイヤー足元の接地判定と地面情報取得を担当するクラス
/// </summary>
public class PlayerGroundProbe
{
    private readonly Transform playerTransform;
    private readonly PlayerConfig config;

    public PlayerGroundProbe(Transform playerTransform, PlayerConfig config)
    {
        this.playerTransform = playerTransform;
        this.config = config;
    }

    public bool IsGrounded()
    {
        return TryGetGroundHit(playerTransform.position, out _);
    }

    public bool TryGetGroundNormal(out Vector3 groundNormal)
    {
        if (TryGetGroundHit(playerTransform.position, out RaycastHit hit))
        {
            groundNormal = hit.normal;
            return true;
        }

        groundNormal = Vector3.up;
        return false;
    }

    public bool TryGetGroundHit(Vector3 referencePosition, out RaycastHit hit)
    {
        Vector3 rayOrigin = referencePosition + Vector3.up * config.GroundRayStartOffset;
        return Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out hit,
            config.GroundCheckDistance,
            config.GroundLayers,
            QueryTriggerInteraction.Ignore);
    }
}
