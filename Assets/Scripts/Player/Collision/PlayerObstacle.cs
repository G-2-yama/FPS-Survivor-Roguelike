using UnityEngine;

/// <summary>プレイヤーを物理的に遮り、接触領域からの押し出し対象にも含めない例外オブジェクトへ付ける</summary>
[DisallowMultipleComponent]
public sealed class PlayerObstacle : MonoBehaviour
{
    private const string ObstacleLayerName = "PlayerObstacle";

    /// <summary>生成時に自身と子のColliderへ専用レイヤーを適用する</summary>
    private void Awake()
    {
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

        foreach (Collider targetCollider in GetComponentsInChildren<Collider>(true))
            targetCollider.gameObject.layer = obstacleLayer;
    }
}
