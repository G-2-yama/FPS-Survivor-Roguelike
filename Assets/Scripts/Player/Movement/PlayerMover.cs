using UnityEngine;

public class PlayerMover
{
    /// <summary>
    /// 停止入力とみなす最小入力値の二乗
    /// </summary>
    private const float StopInputDeadzoneSqr = 0.0001f;
    private const int MaxJumpCount = 2;

    /// <summary>
    /// 接地中に地面へ吸い付けるための下向き速度
    /// </summary>
    private const float GroundedVerticalVelocity = -2f;

    /// <summary>
    /// 空中で逆方向入力を入れた際に水平速度が0になるまでの目安時間
    /// </summary>
    private const float AirReverseToZeroTime = 0.5f;

    /// <summary>
    /// 移動計算に使用するCharacterController
    /// </summary>
    private CharacterController characterController;

    /// <summary>
    /// 移動方向の基準となるプレイヤーTransform
    /// </summary>
    private Transform playerTransform;

    /// <summary>
    /// プレイヤー移動設定
    /// </summary>
    private PlayerConfig config;

    /// <summary>
    /// 水平方向の現在速度
    /// </summary>
    private Vector3 horizontalVelocity;

    /// <summary>
    /// 垂直方向の現在速度
    /// </summary>
    private float verticalVelocity;

    /// <summary>
    /// 長押しジャンプで追加できる残り上向き速度
    /// </summary>
    private float remainingJumpHoldBonus;

    /// <summary>
    /// 長押しジャンプ補正を適用できる残り時間
    /// </summary>
    private float jumpHoldTimer;
    private bool wasGrounded;
    private int remainingJumpCount = MaxJumpCount;

    public PlayerMover(
        Transform playerTransform,
        CharacterController characterController,
        PlayerConfig config)
    {
        this.playerTransform = playerTransform;
        this.characterController = characterController;
        this.config = config;
    }

    /// <summary>
    /// 入力状態に応じて水平移動とジャンプ中の垂直移動を更新する
    /// </summary>
    public void Move(Vector2 moveInput, bool jumpHeld, float deltaTime)
    {
        bool isGrounded = IsGrounded();

        float speed = config.WalkSpeed;
        Vector3 move = playerTransform.right * moveInput.x + playerTransform.forward * moveInput.y;

        if (move.sqrMagnitude > 1f)
        {
            move.Normalize();
        }

        Vector3 targetHorizontalVelocity = move * speed;

        if (move.sqrMagnitude > StopInputDeadzoneSqr)
        {
            if (isGrounded)
            {
                horizontalVelocity = targetHorizontalVelocity;
            }
            else
            {
                float airAcceleration = speed / AirReverseToZeroTime;
                horizontalVelocity = Vector3.MoveTowards(
                    horizontalVelocity,
                    targetHorizontalVelocity,
                    airAcceleration * deltaTime
                );
            }
        }
        else
        {
            if (isGrounded)
            {
                horizontalVelocity = Vector3.MoveTowards(
                    horizontalVelocity,
                    Vector3.zero,
                    config.GroundDeceleration * deltaTime
                );
            }
        }

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = GroundedVerticalVelocity;
            remainingJumpHoldBonus = 0f;
            jumpHoldTimer = 0f;
        }

        if (!isGrounded && jumpHeld && verticalVelocity > 0f && jumpHoldTimer > 0f && remainingJumpHoldBonus > 0f)
        {
            float holdBonusPerSecond = config.JumpForce * (config.JumpHoldMaxMultiplier - 1f)
                / config.JumpHoldDuration;
            float appliedBonus = Mathf.Min(holdBonusPerSecond * deltaTime, remainingJumpHoldBonus);
            verticalVelocity += appliedBonus;
            remainingJumpHoldBonus -= appliedBonus;
            jumpHoldTimer = Mathf.Max(0f, jumpHoldTimer - deltaTime);
        }

        verticalVelocity += config.Gravity * deltaTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        characterController.Move(velocity * deltaTime);

        if (IsGrounded() && verticalVelocity < 0f)
        {
            verticalVelocity = GroundedVerticalVelocity;
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
        Vector3 move = playerTransform.right * moveInput.x + playerTransform.forward * moveInput.y;
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
        float dashSpeed = config.DashDistance / config.DashDuration;

        if (IsGrounded() && verticalVelocity < 0f)
        {
            verticalVelocity = GroundedVerticalVelocity;
        }
        else
        {
            verticalVelocity += config.Gravity * deltaTime;
        }

        Vector3 velocity = direction * dashSpeed + Vector3.up * verticalVelocity;
        characterController.Move(velocity * deltaTime);
    }

    /// <summary>
    /// 強制移動状態へ入る前に水平方向の速度を初期化する
    /// </summary>
    public void ClearHorizontalVelocity()
    {
        horizontalVelocity = Vector3.zero;
    }

    /// <summary>
    /// ジャンプ開始時の上向き速度と長押し補正を初期化する
    /// </summary>
    public void Jump()
    {
        verticalVelocity = config.JumpForce;
        remainingJumpHoldBonus = config.JumpForce * (config.JumpHoldMaxMultiplier - 1f);
        jumpHoldTimer = config.JumpHoldDuration;
    }

    public void RefreshGroundState()
    {
        bool isGrounded = IsGrounded();

        if (isGrounded && !wasGrounded)
        {
            remainingJumpCount = MaxJumpCount;
        }
        else if (!isGrounded && wasGrounded && remainingJumpCount == MaxJumpCount)
        {
            remainingJumpCount = MaxJumpCount - 1;
        }

        wasGrounded = isGrounded;
    }

    public bool TryJump()
    {
        if (remainingJumpCount <= 0)
        {
            return false;
        }

        remainingJumpCount--;
        Jump();
        return true;
    }

    public bool IsGrounded()
    {
        return characterController.isGrounded;
    }

    public void ResolveWallHit(Vector3 wallNormal)
    {
        if (wallNormal.y > 0.1f)
        {
            return;
        }

        horizontalVelocity = Vector3.ProjectOnPlane(horizontalVelocity, wallNormal);
    }

    public void Stop()
    {
        horizontalVelocity = Vector3.zero;
        verticalVelocity = 0f;
        remainingJumpHoldBonus = 0f;
        jumpHoldTimer = 0f;
        wasGrounded = false;
        remainingJumpCount = 0;
    }
}
