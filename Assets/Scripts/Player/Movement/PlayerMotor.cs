using UnityEngine;

/// <summary>
/// CharacterControllerを使ってプレイヤーの移動速度とジャンプ挙動を適用するクラス
/// </summary>
public class PlayerMotor
{
    /// <summary>
    /// 停止入力とみなす最小入力値の二乗
    /// </summary>
    private const float StopInputDeadzoneSqr = 0.0001f;

    /// <summary>
    /// 地上接地で回復する最大ジャンプ回数
    /// </summary>
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
    private PlayerConfig settings;

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

    /// <summary>
    /// 前フレーム時点で接地していたかどうか
    /// </summary>
    private bool wasGrounded;

    /// <summary>
    /// 現在残っているジャンプ可能回数
    /// </summary>
    private int remainingJumpCount = MaxJumpCount;

    /// <summary>
    /// プレイヤー移動処理に必要な参照を初期化する
    /// </summary>
    /// <param name="playerTransform">移動方向の基準になるプレイヤーTransform</param>
    /// <param name="characterController">移動を適用するCharacterController</param>
    /// <param name="settings">移動・ジャンプ設定</param>
    public PlayerMotor(
        Transform playerTransform,
        CharacterController characterController,
        PlayerConfig settings)
    {
        this.playerTransform = playerTransform;
        this.characterController = characterController;
        this.settings = settings;
    }

    /// <summary>
    /// 入力状態に応じて水平移動とジャンプ中の垂直移動を更新する
    /// </summary>
    public void Move(Vector2 moveInput, bool jumpHeld, float deltaTime)
    {
        bool isGrounded = IsGrounded();

        float speed = settings.WalkSpeed;
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
                // 空中では入力方向へ即時反転せず、一定時間で近づける
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
                    settings.GroundDeceleration * deltaTime
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
            // ボタン長押し中だけ残り補正量を少しずつ上向き速度へ足す
            float holdBonusPerSecond = settings.JumpForce * (settings.JumpHoldMaxMultiplier - 1f)
                / settings.JumpHoldDuration;
            float appliedBonus = Mathf.Min(holdBonusPerSecond * deltaTime, remainingJumpHoldBonus);
            verticalVelocity += appliedBonus;
            remainingJumpHoldBonus -= appliedBonus;
            jumpHoldTimer = Mathf.Max(0f, jumpHoldTimer - deltaTime);
        }

        verticalVelocity += settings.Gravity * deltaTime;

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
        float dashSpeed = settings.DashDistance / settings.DashDuration;

        if (IsGrounded() && verticalVelocity < 0f)
        {
            verticalVelocity = GroundedVerticalVelocity;
        }
        else
        {
            verticalVelocity += settings.Gravity * deltaTime;
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
        verticalVelocity = settings.JumpForce;
        remainingJumpHoldBonus = settings.JumpForce * (settings.JumpHoldMaxMultiplier - 1f);
        jumpHoldTimer = settings.JumpHoldDuration;
    }

    /// <summary>
    /// 接地遷移に応じてジャンプ回数を回復・消費する
    /// </summary>
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

    /// <summary>
    /// 残りジャンプ回数があればジャンプを開始する
    /// </summary>
    /// <returns>ジャンプを開始できた場合はtrue</returns>
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

    /// <summary>
    /// CharacterControllerの接地状態を取得する
    /// </summary>
    /// <returns>接地している場合はtrue</returns>
    public bool IsGrounded()
    {
        return characterController.isGrounded;
    }

    /// <summary>
    /// 壁面へ向かう水平速度を壁に沿う方向へ補正する
    /// </summary>
    /// <param name="wallNormal">接触した壁の法線</param>
    public void ResolveWallHit(Vector3 wallNormal)
    {
        if (wallNormal.y > 0.1f)
        {
            return;
        }

        horizontalVelocity = Vector3.ProjectOnPlane(horizontalVelocity, wallNormal);
    }

    /// <summary>
    /// 移動速度とジャンプ関連の実行時状態を停止状態へ戻す
    /// </summary>
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
