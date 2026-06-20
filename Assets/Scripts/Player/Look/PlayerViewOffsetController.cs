using UnityEngine;

/// <summary>
/// カメラ支点の高さオフセットを状態に応じて切り替えるクラス
/// </summary>
public class PlayerViewOffsetController
{
    private Transform cameraLookPivotTransform;
    private PlayerConfig settings;
    private Vector3 defaultLocalPosition;
    private bool slideActive;

    public PlayerViewOffsetController(
        Transform cameraLookPivotTransform,
        PlayerConfig settings)
    {
        this.cameraLookPivotTransform = cameraLookPivotTransform;
        this.settings = settings;
        defaultLocalPosition = cameraLookPivotTransform.localPosition;
    }

    public void SetSlideActive(bool isActive)
    {
        slideActive = isActive;
    }

    public void Update(float deltaTime)
    {
        Vector3 localPosition = cameraLookPivotTransform.localPosition;
        float targetY = slideActive
            ? defaultLocalPosition.y * settings.SlideCameraHeightMultiplier
            : defaultLocalPosition.y;

        float nextY = Mathf.Lerp(
            localPosition.y,
            targetY,
            1f - Mathf.Exp(-settings.SlideCameraHeightLerpSpeed * deltaTime));

        localPosition.x = defaultLocalPosition.x;
        localPosition.z = defaultLocalPosition.z;
        localPosition.y = nextY;
        cameraLookPivotTransform.localPosition = localPosition;
    }
}
