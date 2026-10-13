using UnityEngine;

/// <summary>高さを変えずに対象を円形の水平境界へ押し出す計算を提供する</summary>
public static class PlayerPushSolver
{
    /// <summary>重なりを解消する位置を計算し、対象の垂直位置を維持する</summary>
    public static Vector3 CorrectPosition(Vector3 playerCenter, Vector3 targetPosition, float minimumDistance, Vector3 fallback)
    {
        Vector3 offset = targetPosition - playerCenter;
        offset.y = 0f;
        float distance = offset.magnitude;
        if (distance >= minimumDistance) return targetPosition;
        // 中心が一致すると方向を正規化できないため、対象の向きか右方向を使う
        fallback.y = 0f;
        Vector3 direction = distance > 0.0001f ? offset / distance
            : fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector3.right;
        return targetPosition + direction * (minimumDistance - distance);
    }
}
