using UnityEngine;

/// <summary>
/// プレイヤーの水平回転とカメラの上下回転を制御するクラス
/// </summary>
public class PlayerLookController
{
    /// <summary>
    /// 水平回転を適用するプレイヤーTransform
    /// </summary>
    private Transform playerTransform;

    /// <summary>
    /// 上下回転を適用するカメラのピッチ用Transform
    /// </summary>
    private Transform cameraPitchTransform;

    /// <summary>
    /// 視点感度とピッチ制限の設定
    /// </summary>
    private PlayerConfig settings;

    /// <summary>
    /// 現在のカメラピッチ角度
    /// </summary>
    private float pitch;

    /// <summary>
    /// 武器反動として一時的に視点へ加える回転量
    /// </summary>
    private Vector2 recoilOffset;

    /// <summary>
    /// 視点制御に必要な参照を初期化する
    /// </summary>
    /// <param name="playerTransform">水平回転を適用するTransform</param>
    /// <param name="cameraPitchTransform">上下回転を適用するTransform</param>
    /// <param name="settings">視点設定</param>
    public PlayerLookController(
        Transform playerTransform,
        Transform cameraPitchTransform,
        PlayerConfig settings)
    {
        this.playerTransform = playerTransform;
        this.cameraPitchTransform = cameraPitchTransform;
        this.settings = settings;
    }

    /// <summary>
    /// 視点移動を適用する
    /// </summary>
    /// <param name="lookInput">視点操作入力</param>
    public void ApplyLook(Vector2 lookInput)
    {
        float yawDelta = lookInput.x * settings.LookSensitivity;
        float pitchDelta = lookInput.y * settings.LookSensitivity;

        // 反動を加算
        yawDelta += recoilOffset.x;
        pitchDelta += recoilOffset.y;

        playerTransform.Rotate(Vector3.up * yawDelta);

        pitch = Mathf.Clamp(pitch - pitchDelta, settings.MinPitch, settings.MaxPitch);
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
