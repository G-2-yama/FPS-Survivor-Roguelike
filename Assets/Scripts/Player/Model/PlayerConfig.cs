using UnityEngine;

/// <summary>
/// プレイヤーの移動・視点に関する設定値と入力状態を保持するモデル
/// </summary>
public class PlayerConfig : MonoBehaviour
{
    /// <summary>
    /// 通常移動速度の設定値
    /// </summary>
    [SerializeField] private float walkSpeed = 5f;

    /// <summary>
    /// 通常移動時の速度
    /// </summary>
    public float WalkSpeed => walkSpeed;

    /// <summary>
    /// 走行速度の設定値
    /// </summary>
    [SerializeField] private float runSpeed = 8f;

    /// <summary>
    /// 走行時の速度
    /// </summary>
    public float RunSpeed => runSpeed;

    /// <summary>
    /// 視点感度の設定値
    /// </summary>
    [SerializeField] private float lookSensitivity = 0.08f;

    /// <summary>
    /// 視点移動の感度
    /// </summary>
    public float LookSensitivity => lookSensitivity;

    /// <summary>
    /// ピッチ最小角度の設定値
    /// </summary>
    [SerializeField] private float minPitch = -80f;

    /// <summary>
    /// 上下視点の最小角度
    /// </summary>
    public float MinPitch => minPitch;

    /// <summary>
    /// ピッチ最大角度の設定値
    /// </summary>
    [SerializeField] private float maxPitch = 80f;

    /// <summary>
    /// 上下視点の最大角度
    /// </summary>
    public float MaxPitch => maxPitch;
}
