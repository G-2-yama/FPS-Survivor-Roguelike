using UnityEngine;

public class PlayerLook
{
    private Transform playerTransform;
    private Transform cameraPitchTransform;
    private PlayerConfig config;

    private float pitch;

    private Vector2 recoilOffset;

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

        // 反動を加算
        yawDelta += recoilOffset.x;
        pitchDelta += recoilOffset.y;

        playerTransform.Rotate(Vector3.up * yawDelta);

        pitch = Mathf.Clamp(pitch - pitchDelta, config.MinPitch, config.MaxPitch);
        cameraPitchTransform.localEulerAngles = new Vector3(pitch, 0, 0);

        // 反動を徐々に減衰させる
        recoilOffset = Vector2.Lerp(recoilOffset, Vector2.zero, Time.deltaTime * 10f);
    }

    /// <summary>
    /// 反動を加える
    /// </summary>
    /// <param name="recoil">反動</param>
    public void AddRecoil(Vector2 recoil)
    {
        recoilOffset += recoil;
    }

}
