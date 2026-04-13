using UnityEngine;

/// <summary>
/// プレイヤーの移動・視点・ジャンプ・ダッシュ・初期HPに関する設定値を保持するモデル
/// </summary>
public class PlayerConfig : MonoBehaviour
{
    /// <summary>
    /// 通常移動速度の設定値
    /// </summary>
    [SerializeField] private float walkSpeed = 10f;

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
    /// 地上で入力を離した時の減速量
    /// </summary>
    [SerializeField] private float groundDeceleration = 30f;

    /// <summary>
    /// 地上で入力を離した時の減速量
    /// </summary>
    public float GroundDeceleration => groundDeceleration;

    /// <summary>
    /// 地上で速度を目標速度へ近づける加速量
    /// </summary>
    [SerializeField] private float groundAcceleration = 24f;

    /// <summary>
    /// 地上で速度を目標速度へ近づける加速量
    /// </summary>
    public float GroundAcceleration => groundAcceleration;

    /// <summary>
    /// 空中で入力を離した後も慣性を残したまま減衰させる強さ
    /// </summary>
    [SerializeField] private float airDeceleration = 45f;

    /// <summary>
    /// 空中で入力を離した後も慣性を残したまま減衰させる強さ
    /// </summary>
    public float AirDeceleration => airDeceleration;

    /// <summary>
    /// ジャンプ長押し補正を受け付ける時間
    /// </summary>
    [SerializeField] private float jumpHoldDuration = 0.20f;

    /// <summary>
    /// ジャンプ長押し補正を受け付ける時間
    /// </summary>
    public float JumpHoldDuration => jumpHoldDuration;

    /// <summary>
    /// ジャンプ長押し時に適用する最大倍率
    /// </summary>
    [SerializeField] private float jumpHoldMaxMultiplier = 1.2f;

    /// <summary>
    /// ジャンプ長押し時に適用する最大倍率
    /// </summary>
    public float JumpHoldMaxMultiplier => jumpHoldMaxMultiplier;

    /// <summary>
    /// 垂直方向へ毎フレーム加算する重力加速度
    /// </summary>
    [SerializeField] private float gravity = -18f;

    /// <summary>
    /// 垂直方向へ毎フレーム加算する重力加速度
    /// </summary>
    public float Gravity => gravity;

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

    /// <summary>
    /// 地面判定レイの開始位置オフセット
    /// </summary>
    [SerializeField] private float groundRayStartOffset = 0.5f;

    /// <summary>
    /// 地面判定レイの開始位置オフセット
    /// </summary>
    public float GroundRayStartOffset => groundRayStartOffset;

    /// <summary>
    /// 地面判定レイの距離
    /// </summary>
    [SerializeField] private float groundCheckDistance = 2f;

    /// <summary>
    /// 地面判定レイの距離
    /// </summary>
    public float GroundCheckDistance => groundCheckDistance;

    /// <summary>
    /// 地面判定対象レイヤー
    /// </summary>
    [SerializeField] private LayerMask groundLayers = ~0;

    /// <summary>
    /// 地面判定対象レイヤー
    /// </summary>
    public LayerMask GroundLayers => groundLayers;

    /// <summary>
    /// ジャンプ時の初速
    /// </summary>
    [SerializeField] private float jumpForce = 8f;

    /// <summary>
    /// ジャンプ時の初速
    /// </summary>
    public float JumpForce => jumpForce;

    /// <summary>
    /// プレイヤーの初期HP
    /// </summary>
    [SerializeField] private int initialHP = 100;

    /// <summary>
    /// プレイヤーの初期HP
    /// </summary>
    public int InitialHP => initialHP;

    /// <summary>
    /// ダッシュで移動する距離
    /// </summary>
    [SerializeField, Min(0f)] private float dashDistance = 6f;

    /// <summary>
    /// ダッシュで移動する距離
    /// </summary>
    public float DashDistance => dashDistance;

    /// <summary>
    /// ダッシュ移動を継続する時間
    /// </summary>
    [SerializeField, Min(0.01f)] private float dashDuration = 0.12f;

    /// <summary>
    /// ダッシュ移動を継続する時間
    /// </summary>
    public float DashDuration => dashDuration;

    /// <summary>
    /// 次のダッシュを開始できるまでの待ち時間
    /// </summary>
    [SerializeField, Min(0f)] private float dashCooldown = 0.5f;

    /// <summary>
    /// 次のダッシュを開始できるまでの待ち時間
    /// </summary>
    public float DashCooldown => dashCooldown;
}
