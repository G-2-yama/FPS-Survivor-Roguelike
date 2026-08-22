using UnityEngine;

/// <summary>
/// プレイヤーの移動・視点・ジャンプ・ダッシュ設定を保持するモデル
/// </summary>
public class PlayerConfig : MonoBehaviour
{
    /// <summary>
    /// 通常移動速度の設定値
    /// </summary>
    [Header("Movement")]
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
    [Header("Jump")]
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
    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.08f;

    /// <summary>
    /// 視点移動の感度
    /// </summary>
    public float LookSensitivity => lookSensitivity;

    /// <summary>
    /// ゲームパッド右スティックの旋回速度（度/秒）
    /// </summary>
    [SerializeField, Min(0f)] private float gamepadLookSensitivity = 60f;

    public float GamepadLookSensitivity => gamepadLookSensitivity;

    /// <summary>
    /// 右スティックのニュートラル付近を無視する範囲
    /// </summary>
    [SerializeField, Range(0f, 0.95f)] private float gamepadLookDeadzone = 0.25f;

    public float GamepadLookDeadzone => gamepadLookDeadzone;

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
    [Header("Ground Check")]
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
    [Header("Stats")]
    [SerializeField] private int initialHP = 100;

    /// <summary>
    /// プレイヤーの初期HP
    /// </summary>
    public int InitialHP => initialHP;

    /// <summary>
    /// ダッシュで移動する距離
    /// </summary>
    [Header("Dash")]
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

    [Header("Slide")]
    [SerializeField, Min(0f)] private float slideSpeed = 12f;
    [SerializeField, Min(0f)] private float slideMinInertiaSpeed = 1.5f;
    [SerializeField, Min(0f)] private float slideTurnRateDegreesPerSecond = 180f;

    [Header("Fast Fall")]
    [SerializeField, Min(0f)] private float fastFallEntrySpeed = 20f;
    [SerializeField, Min(0f)] private float fastFallAcceleration = 90f;
    [SerializeField, Min(0f)] private float fastFallTerminalSpeed = 35f;

    [Header("Slide Camera")]
    [SerializeField, Min(0f)] private float slideCameraHeightMultiplier = 0.5f;
    [SerializeField, Min(0f)] private float slideCameraHeightLerpSpeed = 12f;

    public float SlideSpeed => slideSpeed;
    public float SlideMinInertiaSpeed => slideMinInertiaSpeed;
    public float SlideTurnRateDegreesPerSecond => slideTurnRateDegreesPerSecond;
    public float FastFallEntrySpeed => fastFallEntrySpeed;
    public float FastFallAcceleration => fastFallAcceleration;
    public float FastFallTerminalSpeed => fastFallTerminalSpeed;
    public float SlideCameraHeightMultiplier => slideCameraHeightMultiplier;
    public float SlideCameraHeightLerpSpeed => slideCameraHeightLerpSpeed;
}
