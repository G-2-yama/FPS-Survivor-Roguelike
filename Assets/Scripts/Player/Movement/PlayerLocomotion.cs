using UnityEngine;

/// <summary>
/// 通常移動、空中移動、ダッシュ移動の速度計算を扱うクラス
/// </summary>
public class PlayerLocomotion
{
    /// <summary>
    /// 停止入力とみなす最小入力値の二乗
    /// </summary>
    private const float StopInputDeadzoneSqr = 0.0001f;

    /// <summary>
    /// 接地中に地面へ吸い付けるための下向き速度
    /// </summary>
    private const float GroundedVerticalVelocity = -2f;

    /// <summary>
    /// 空中で逆方向入力を入れた際に水平速度が0になるまでの目安時間
    /// </summary>
    private const float AirReverseToZeroTime = 0.5f;

    /// <summary>
    /// 移動方向の基準となるプレイヤーTransform
    /// </summary>
    private Transform playerTransform;

    /// <summary>
    /// 速度を適用するモーター
    /// </summary>
    private PlayerMotor motor;

    /// <summary>
    /// ジャンプ関連の実行時状態
    /// </summary>
    private PlayerJumpController jumpController;

    /// <summary>
    /// プレイヤー移動設定
    /// </summary>
    private PlayerConfig settings;

    /// <summary>
    /// 移動計算に必要な参照を初期化する
    /// </summary>
    /// <param name="playerTransform">移動方向の基準になるプレイヤーTransform</param>
    /// <param name="motor">速度を適用するモーター</param>
    /// <param name="jumpController">ジャンプ関連の実行時状態</param>
    /// <param name="settings">移動・ジャンプ設定</param>
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
    /// 入力状態に応じて水平移動とジャンプ中の垂直移動を更新する
    /// </summary>
    public void Move(Vector2 moveInput, bool jumpHeld, float deltaTime)
    {
        bool isGrounded = motor.IsGrounded();
        Vector3 move = GetMoveVector(moveInput);
        Vector3 targetHorizontalVelocity = move * settings.WalkSpeed;

        if (move.sqrMagnitude > StopInputDeadzoneSqr)
        {
            if (isGrounded)
            {
                motor.HorizontalVelocity = targetHorizontalVelocity;
            }
            else
            {
                float airAcceleration = settings.WalkSpeed / AirReverseToZeroTime;
                motor.HorizontalVelocity = Vector3.MoveTowards(
                    motor.HorizontalVelocity,
                    targetHorizontalVelocity,
                    airAcceleration * deltaTime
                );
            }
        }
        else if (isGrounded)
        {
            motor.HorizontalVelocity = Vector3.MoveTowards(
                motor.HorizontalVelocity,
                Vector3.zero,
                settings.GroundDeceleration * deltaTime
            );
        }

        if (isGrounded && motor.VerticalVelocity < 0f)
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
    /// 移動入力をワールド座標系の移動方向へ変換する
    /// </summary>
    /// <param name="moveInput">移動入力値</param>
    /// <param name="direction">正規化されたワールド座標系の移動方向</param>
    /// <returns>有効な移動方向を取得できた場合はtrue</returns>
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
    /// 指定された方向へ1フレーム分のダッシュ移動を行う
    /// </summary>
    /// <param name="direction">正規化されたワールド座標系のダッシュ方向</param>
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

    /// <summary>
    /// 強制移動状態へ入る前に水平方向の速度を初期化する
    /// </summary>
    public void ClearHorizontalVelocity()
    {
        motor.HorizontalVelocity = Vector3.zero;
    }

    /// <summary>
    /// 移動入力をプレイヤー基準のワールド移動量に変換する
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
