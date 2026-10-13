using UnityEngine;

/// <summary>プレイヤーを物理的に遮り、接触領域からの押し出し対象にも含めない例外オブジェクトへ付ける</summary>
[DisallowMultipleComponent]
public sealed class PlayerObstacle : MonoBehaviour
{
    private const string ObstacleLayerName = "PlayerObstacle";
    [SerializeField] private Collider[] targetColliders;

    /// <summary>生成時に自身と子のColliderへ専用レイヤーを適用する</summary>
    private void Awake()
    {
        if (targetColliders == null || targetColliders.Length == 0)
            targetColliders = GetComponentsInChildren<Collider>(true);
        ApplyLayerToColliders();
    }

    /// <summary>自身と子のColliderをプレイヤー遮蔽用レイヤーへ設定する</summary>
    public void ApplyLayerToColliders()
    {
        int obstacleLayer = LayerMask.NameToLayer(ObstacleLayerName);
        if (obstacleLayer < 0)
        {
            Debug.LogError($"{ObstacleLayerName}レイヤーがProject Settingsに登録されていません", this);
            return;
        }

        if (targetColliders == null) return;
        foreach (Collider targetCollider in targetColliders)
            if (targetCollider != null) targetCollider.gameObject.layer = obstacleLayer;
    }
}
