using UnityEngine;

public class PlayerLook
{
    private Transform playerTransform;
    private Transform cameraPitchTransform;
    private PlayerConfig config;

    private float pitch;

    public PlayerLook(Transform playerTransform, Transform cameraPitchTransform, PlayerConfig config)
    {
        this.playerTransform = playerTransform;
        this.cameraPitchTransform = cameraPitchTransform;
        this.config = config;
    }

    /// <summary>
    /// 視点移動を適用する
    /// </summary>
    /// <param name="lookInput">視点操作入力</param>
    public void ApplyLook(Vector2 lookInput)
    {
        float yawDelta = lookInput.x * config.LookSensitivity;
        float pitchDelta = lookInput.y * config.LookSensitivity;

        playerTransform.Rotate(Vector3.up * yawDelta);

        pitch = Mathf.Clamp(pitch - pitchDelta, config.MinPitch, config.MaxPitch);
        cameraPitchTransform.localEulerAngles = new Vector3(pitch, 0, 0);
    }
}
