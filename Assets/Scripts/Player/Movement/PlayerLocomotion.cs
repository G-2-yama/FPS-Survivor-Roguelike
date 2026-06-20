using UnityEngine;

/// <summary>
/// プレイヤーの継続的な移動性能を扱うクラス。
/// ここでいう Locomotion は姿勢そのものではなく、歩行中や空中移動中の速度更新を指す。
/// </summary>
public class PlayerLocomotion
{
    /// <summary>
    /// 停止扱いにする最小の入力量
    /// </summary>
    private const float StopInputDeadzoneSqr = 0.0001f;

    /// <summary>
    /// 接地中に床へ吸い付けるための下向き速度
    /// </summary>
    private const float GroundedVerticalVelocity = -2f;

    /// <summary>
    /// 空中で逆方向へ切り返す時にゼロまで戻す目安時間
    /// </summary>
    private const float AirReverseToZeroTime = 0.5f;

    /// <summary>
    /// 移動入力をワールド座標へ変換する基準Transform
    /// </summary>
    private Transform playerTransform;

    /// <summary>
    /// 速度を適用するモーター
    /// </summary>
    private PlayerMotor motor;

    /// <summary>
    /// ジャンプ補正を扱う制御クラス
    /// </summary>
    private PlayerJumpController jumpController;

    /// <summary>
    /// プレイヤー移動設定
    /// </summary>
    private PlayerConfig settings;

    /// <summary>
    /// 移動制御に必要な参照を初期化する
    /// </summary>
    /// <param name="playerTransform">移動方向を決めるTransform</param>
    /// <param name="motor">速度を適用するモーター</param>
    /// <param name="jumpController">ジャンプ補正を扱う制御クラス</param>
    /// <param name="settings">プレイヤー移動設定</param>
    public PlayerLocomotion(
        Transform playerTransform,
        PlayerMotor motor,
        PlayerJumpController jumpController,
        PlayerConfig settings)
    {
        this.playerTransform = playerTransform;
        this.motor = motor;
        this.jumpController = jumpController;
        this.settings = settings;
    }

    /// <summary>
    /// 通常移動を更新し、垂直方向の速度とジャンプ長押しも進める
    /// </summary>
    public void Move(Vector2 moveInput, bool jumpHeld, bool sprintHeld, float deltaTime)
    {
        bool isGrounded = motor.IsGrounded();
        bool isEffectivelyGrounded = isGrounded && motor.VerticalVelocity <= 0f;
        Vector3 move = GetMoveVector(moveInput);
        float moveSpeed = sprintHeld ? settings.RunSpeed : settings.WalkSpeed;
        Vector3 targetHorizontalVelocity = move * moveSpeed;

        if (move.sqrMagnitude > StopInputDeadzoneSqr)
        {
            if (isEffectivelyGrounded)
            {
                motor.HorizontalVelocity = targetHorizontalVelocity;
            }
            else
            {
                float airAcceleration = moveSpeed / AirReverseToZeroTime;
                float currentHorizontalSpeed = motor.HorizontalVelocity.magnitude;
                float targetHorizontalSpeed = Mathf.Max(moveSpeed, currentHorizontalSpeed);
                Vector3 airborneTargetVelocity = move * targetHorizontalSpeed;

                motor.HorizontalVelocity = Vector3.MoveTowards(
                    motor.HorizontalVelocity,
                    airborneTargetVelocity,
                    airAcceleration * deltaTime
                );
            }
        }
        else if (isEffectivelyGrounded)
        {
            motor.HorizontalVelocity = Vector3.MoveTowards(
                motor.HorizontalVelocity,
                Vector3.zero,
                settings.GroundDeceleration * deltaTime
            );
        }

        if (isEffectivelyGrounded && motor.VerticalVelocity < 0f)
        {
            motor.VerticalVelocity = GroundedVerticalVelocity;
            jumpController.ClearJumpHold();
        }

        jumpController.ApplyJumpHold(jumpHeld, deltaTime);
        motor.VerticalVelocity += settings.Gravity * deltaTime;
        motor.Move(deltaTime);

        if (motor.IsGrounded() && motor.VerticalVelocity < 0f)
        {
            motor.VerticalVelocity = GroundedVerticalVelocity;
        }
    }

    /// <summary>
    /// ダッシュ開始に使える移動方向を取得する
    /// </summary>
    /// <param name="moveInput">移動入力</param>
    /// <param name="direction">正規化した移動方向</param>
    /// <returns>有効な移動方向がある場合はtrue</returns>
    public bool TryGetMoveDirection(Vector2 moveInput, out Vector3 direction)
    {
        Vector3 move = GetMoveVector(moveInput);
        if (move.sqrMagnitude <= StopInputDeadzoneSqr)
        {
            direction = Vector3.zero;
            return false;
        }

        direction = move.normalized;
        return true;
    }

    /// <summary>
    /// 指定方向へ1フレーム分のダッシュ移動を適用する
    /// </summary>
    /// <param name="direction">正規化されたダッシュ方向</param>
    /// <param name="deltaTime">ダッシュ移動を適用する時間</param>
    public void MoveDash(Vector3 direction, float deltaTime)
    {
        float dashSpeed = settings.DashDistance / settings.DashDuration;

        if (motor.IsGrounded() && motor.VerticalVelocity < 0f)
        {
            motor.VerticalVelocity = GroundedVerticalVelocity;
        }
        else
        {
            motor.VerticalVelocity += settings.Gravity * deltaTime;
        }

        motor.MoveWithHorizontalVelocity(direction * dashSpeed, deltaTime);
    }

    public void MoveSlide(Vector3 direction, float speed, float deltaTime)
    {
        if (motor.IsGrounded() && motor.VerticalVelocity < 0f)
        {
            motor.VerticalVelocity = GroundedVerticalVelocity;
        }
        else
        {
            motor.VerticalVelocity += settings.Gravity * deltaTime;
        }

        motor.MoveWithHorizontalVelocity(direction * speed, deltaTime);

        if (motor.IsGrounded() && motor.VerticalVelocity < 0f)
        {
            motor.VerticalVelocity = GroundedVerticalVelocity;
        }
    }

    /// <summary>
    /// ダッシュ開始前などに水平速度を明示的に止める
    /// </summary>
    public void ClearHorizontalVelocity()
    {
        motor.HorizontalVelocity = Vector3.zero;
    }

    /// <summary>
    /// 移動入力をプレイヤー基準のワールド移動へ変換する
    /// </summary>
    private Vector3 GetMoveVector(Vector2 moveInput)
    {
        Vector3 move = playerTransform.right * moveInput.x + playerTransform.forward * moveInput.y;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        return move;
    }
}
