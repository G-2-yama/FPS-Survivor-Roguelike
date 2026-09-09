using UnityEngine;

/// <summary>
/// 移動状態に応じてカメラの視野角を滑らかに制御する
/// </summary>
public class PlayerFovController
{
    private const float MinimumFieldOfView = 1f;
    private const float MaximumFieldOfView = 179f;

    private readonly Camera playerCamera;
    private readonly PlayerConfig settings;
    private readonly float defaultFieldOfView;

    public PlayerFovController(Camera playerCamera, PlayerConfig settings)
    {
        this.playerCamera = playerCamera;
        this.settings = settings;
        defaultFieldOfView = playerCamera != null ? playerCamera.fieldOfView : 0f;
    }

    /// <summary>
    /// スプリントとスライドの状態に合わせて視野角を更新する
    /// </summary>
    public void Update(bool isSprinting, bool isSliding, float deltaTime)
    {
        if (playerCamera == null || settings == null)
        {
            return;
        }

        float increase = isSliding
            ? settings.SlideFieldOfViewIncrease
            : isSprinting
                ? settings.SprintFieldOfViewIncrease
                : 0f;

        float targetFieldOfView = Mathf.Clamp(
            defaultFieldOfView + increase,
            MinimumFieldOfView,
            MaximumFieldOfView);

        float lerpSpeed = targetFieldOfView > playerCamera.fieldOfView
            ? settings.FieldOfViewIncreaseLerpSpeed
            : settings.FieldOfViewDecreaseLerpSpeed;

        float interpolation = 1f - Mathf.Exp(-lerpSpeed * deltaTime);
        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFieldOfView,
            interpolation);
    }

    /// <summary>
    /// 視野角を開始時の値へ即座に戻す
    /// </summary>
    public void Reset()
    {
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = defaultFieldOfView;
        }
    }
}
