using UnityEngine;

/// <summary>
/// ジャンプ回数、ジャンプ開始、長押しジャンプ補正を扱うクラス
/// </summary>
public class PlayerJumpController
{
    /// <summary>
    /// 地上接地で回復する最大ジャンプ回数
    /// </summary>
    private const int MaxJumpCount = 2;

    /// <summary>
    /// 速度を適用するモーター
    /// </summary>
    private PlayerMotor motor;

    /// <summary>
    /// プレイヤー移動設定
    /// </summary>
    private PlayerConfig settings;

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
    /// ジャンプ処理に必要な参照を初期化する
    /// </summary>
    /// <param name="motor">速度を適用するモーター</param>
    /// <param name="settings">ジャンプ設定</param>
    public PlayerJumpController(PlayerMotor motor, PlayerConfig settings)
    {
        this.motor = motor;
        this.settings = settings;
    }

    /// <summary>
    /// 接地遷移に応じてジャンプ回数を回復・消費する
    /// </summary>
    public void RefreshGroundState()
    {
        bool isGrounded = motor.IsGrounded();

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
    /// ジャンプ開始時の上向き速度と長押し補正を初期化する
    /// </summary>
    public void Jump()
    {
        motor.VerticalVelocity = settings.JumpForce;
        remainingJumpHoldBonus = settings.JumpForce * (settings.JumpHoldMaxMultiplier - 1f);
        jumpHoldTimer = settings.JumpHoldDuration;
    }

    /// <summary>
    /// ジャンプ長押し補正を現在の垂直速度へ反映する
    /// </summary>
    /// <param name="jumpHeld">ジャンプ入力を押し続けているかどうか</param>
    /// <param name="deltaTime">前フレームからの経過時間</param>
    public void ApplyJumpHold(bool jumpHeld, float deltaTime)
    {
        if (motor.IsGrounded()
            || !jumpHeld
            || motor.VerticalVelocity <= 0f
            || jumpHoldTimer <= 0f
            || remainingJumpHoldBonus <= 0f)
        {
            return;
        }

        float holdBonusPerSecond = settings.JumpForce * (settings.JumpHoldMaxMultiplier - 1f)
            / settings.JumpHoldDuration;
        float appliedBonus = Mathf.Min(holdBonusPerSecond * deltaTime, remainingJumpHoldBonus);
        motor.VerticalVelocity += appliedBonus;
        remainingJumpHoldBonus -= appliedBonus;
        jumpHoldTimer = Mathf.Max(0f, jumpHoldTimer - deltaTime);
    }

    /// <summary>
    /// 長押しジャンプ補正を終了する
    /// </summary>
    public void ClearJumpHold()
    {
        remainingJumpHoldBonus = 0f;
        jumpHoldTimer = 0f;
    }

    /// <summary>
    /// ジャンプ関連の実行時状態を停止状態へ戻す
    /// </summary>
    public void Stop()
    {
        remainingJumpHoldBonus = 0f;
        jumpHoldTimer = 0f;
        wasGrounded = false;
        remainingJumpCount = 0;
    }
}
