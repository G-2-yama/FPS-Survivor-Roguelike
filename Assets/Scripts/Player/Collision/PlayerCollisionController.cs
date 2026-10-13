using UnityEngine;

/// <summary>SOの本体寸法を反映し、敵との接触領域を構築する</summary>
[DisallowMultipleComponent, RequireComponent(typeof(CharacterController))]
public sealed class PlayerCollisionController : MonoBehaviour
{
    private PlayerCollisionSettings settings;
    private CharacterController controller;
    private PlayerEnemyPushZone pushZone;
    private CapsuleCollider bodyTrigger;

    /// <summary>開始時の設定を本体と敵の接触領域へ反映する</summary>
    public void Initialize(Player player, PlayerCollisionSettings collisionSettings)
    {
        settings = collisionSettings;
        controller = GetComponent<CharacterController>();
        // 一度無効化して全寸法を反映し、中間的に高さと段差の条件が崩れるのを防ぐ
        bool wasEnabled = controller.enabled;
        controller.enabled = false;
        controller.radius = settings.Radius;
        controller.height = settings.Height;
        controller.center = settings.Center;
        controller.slopeLimit = settings.SlopeLimit;
        controller.stepOffset = settings.StepOffset;
        controller.skinWidth = settings.SkinWidth;
        controller.minMoveDistance = settings.MinMoveDistance;
        gameObject.layer = settings.BodyLayer;
        // プレイヤー単位の除外設定を使い、他の物理オブジェクトの衝突設定を変えない
        controller.excludeLayers |= settings.EnemyLayers;
        controller.excludeLayers |= 1 << settings.PushZoneLayer;
        controller.layerOverridePriority = 10;
        controller.enabled = wasEnabled;
        bodyTrigger = GetComponent<CapsuleCollider>();
        SyncBodyTrigger();

        Transform zoneTransform = transform.Find("EnemyPushZone");
        if (zoneTransform == null)
        {
            zoneTransform = new GameObject("EnemyPushZone").transform;
            zoneTransform.SetParent(transform, false);
        }
        zoneTransform.localPosition = Vector3.zero;
        zoneTransform.localRotation = Quaternion.identity;
        zoneTransform.localScale = Vector3.one;
        if (!zoneTransform.TryGetComponent(out pushZone))
            pushZone = zoneTransform.gameObject.AddComponent<PlayerEnemyPushZone>();
        pushZone.Initialize(player, controller, settings);
    }

    /// <summary>既存の被弾用カプセルも本体の姿勢変更に追従させる</summary>
    private void FixedUpdate() => SyncBodyTrigger();

    /// <summary>既存の被弾用Colliderを本体寸法に統一し、物理的な押し合いを防ぐ</summary>
    private void SyncBodyTrigger()
    {
        if (bodyTrigger == null || controller == null) return;
        bodyTrigger.isTrigger = true;
        bodyTrigger.direction = 1;
        bodyTrigger.radius = controller.radius;
        bodyTrigger.height = controller.height;
        bodyTrigger.center = controller.center;
    }

    /// <summary>選択時にSOの本体と接触領域を表示する</summary>
    private void OnDrawGizmosSelected()
    {
        PlayerConfig config = GetComponent<PlayerConfig>();
        PlayerCollisionSettings preview = settings != null ? settings : config != null ? config.Collision : null;
        if (preview == null) return;
        CharacterController current = GetComponent<CharacterController>();
        Vector3 center = Application.isPlaying && current != null ? current.center : preview.Center;
        float height = Application.isPlaying && current != null ? current.height : preview.Height;
        float radius = Application.isPlaying && current != null ? current.radius : preview.Radius;
        Matrix4x4 previous = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        DrawCapsule(center, height, radius, Color.cyan);
        DrawCapsule(center, Mathf.Max(height, (radius + preview.PushRadiusPadding) * 2f),
            radius + preview.PushRadiusPadding, Color.yellow);
        Gizmos.matrix = previous;
    }

    /// <summary>本体と同じカプセル形状の寸法を線で描画する</summary>
    private static void DrawCapsule(Vector3 center, float height, float radius, Color color)
    {
        Gizmos.color = color;
        Vector3 halfAxis = Vector3.up * Mathf.Max(0f, height * 0.5f - radius);
        Gizmos.DrawWireSphere(center + halfAxis, radius);
        Gizmos.DrawWireSphere(center - halfAxis, radius);
        foreach (Vector3 axis in new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back })
            Gizmos.DrawLine(center + halfAxis + axis * radius, center - halfAxis + axis * radius);
    }
}
