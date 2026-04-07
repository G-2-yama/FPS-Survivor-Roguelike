using Unity.VisualScripting;
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
    /// 地上で入力を離した時の減速量
    /// </summary>
    [SerializeField] private float groundDeceleration = 30f;

    /// <summary>
    /// 地上で入力を離した時の減速量
    /// </summary>
    public float GroundDeceleration => groundDeceleration;

    [SerializeField] private float groundAcceleration = 24f;

    public float GroundAcceleration => groundAcceleration;

    [SerializeField] private float gravity = -14f;

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

    public int InitialHP => initialHP;

}
