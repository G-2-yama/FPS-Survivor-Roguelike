using UnityEngine;

/// <summary>
/// カメラ支点の高さオフセットを状態に応じて切り替えるクラス
/// </summary>
public class PlayerViewOffsetController
{
    private Transform cameraLookPivotTransform;
    private PlayerCameraSettings settings;
    private Vector3 defaultLocalPosition;
    private bool slideActive;

    /// <summary>カメラ支点と高さの演出設定を関連付ける</summary>
    public PlayerViewOffsetController(
        Transform cameraLookPivotTransform,
        PlayerCameraSettings settings)
    {
        this.cameraLookPivotTransform = cameraLookPivotTransform;
        this.settings = settings;
        defaultLocalPosition = cameraLookPivotTransform.localPosition;
    }

    /// <summary>スライド用の視点高さを使用するか設定する</summary>
    public void SetSlideActive(bool isActive)
    {
        slideActive = isActive;
    }

    /// <summary>現在の姿勢に応じた視点高さへ滑らかに近づける</summary>
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
