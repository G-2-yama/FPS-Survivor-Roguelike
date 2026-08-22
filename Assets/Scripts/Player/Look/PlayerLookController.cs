using UnityEngine;

/// <summary>
/// プレイヤーの水平視点とカメラの上下視点を制御するクラス
/// </summary>
public class PlayerLookController
{
    /// <summary>
    /// 水平回転を適用するプレイヤーTransform
    /// </summary>
    private Transform playerTransform;

    /// <summary>
    /// 上下視点を適用するカメラの回転支点
    /// </summary>
    private Transform cameraLookPivotTransform;

    /// <summary>
    /// 視点感度とピッチ制限の設定
    /// </summary>
    private PlayerConfig settings;

    /// <summary>
    /// 現在のカメラピッチ角度
    /// </summary>
    private float pitch;

    /// <summary>
    /// 視点制御に必要な参照を初期化する
    /// </summary>
    /// <param name="playerTransform">水平回転を適用するTransform</param>
    /// <param name="cameraLookPivotTransform">上下視点を適用するTransform</param>
    /// <param name="settings">視点設定</param>
    public PlayerLookController(
        Transform playerTransform,
        Transform cameraLookPivotTransform,
        PlayerConfig settings)
    {
        this.playerTransform = playerTransform;
        this.cameraLookPivotTransform = cameraLookPivotTransform;
        this.settings = settings;
    }

    public void ApplyView(Vector2 lookInput, bool isGamepadLookInput, Vector2 recoilOffset, float deltaTime)
    {
        ApplyLook(lookInput, isGamepadLookInput, deltaTime);
        ApplyCameraRotation(recoilOffset);
    }

    /// <summary>
    /// 視点移動を適用する
    /// </summary>
    /// <param name="lookInput">視点操作入力</param>
    private void ApplyLook(Vector2 lookInput, bool isGamepadLookInput, float deltaTime)
    {
        float sensitivity = settings.LookSensitivity;

        if (isGamepadLookInput)
        {
            lookInput = ApplyDeadzone(lookInput, settings.GamepadLookDeadzone);
            sensitivity = settings.GamepadLookSensitivity * deltaTime;
        }

        float yawDelta = lookInput.x * sensitivity;
        float pitchDelta = lookInput.y * sensitivity;

        playerTransform.Rotate(Vector3.up * yawDelta);

        pitch = Mathf.Clamp(
            pitch - pitchDelta,
            settings.MinPitch,
            settings.MaxPitch);
    }

    private static Vector2 ApplyDeadzone(Vector2 input, float deadzone)
    {
        float magnitude = Mathf.Clamp01(input.magnitude);
        if (magnitude <= deadzone)
        {
            return Vector2.zero;
        }

        float scaledMagnitude = Mathf.Clamp01((magnitude - deadzone) / (1f - deadzone));
        return input.normalized * scaledMagnitude;
    }

    private void ApplyCameraRotation(Vector2 recoilOffset)
    {
        Quaternion lookRotation =
            Quaternion.Euler(pitch, 0, 0);

        Quaternion recoilRotation =
            Quaternion.Euler(-recoilOffset.y, recoilOffset.x, 0);

        cameraLookPivotTransform.localRotation =
            lookRotation * recoilRotation;
    }
}
