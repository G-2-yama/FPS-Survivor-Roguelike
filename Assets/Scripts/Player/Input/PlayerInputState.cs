using UnityEngine;

/// <summary>
/// プレイヤー入力の現在値と1回だけ消費する入力要求を保持するモデル
/// </summary>
public class PlayerInputState
{
    /// <summary>
    /// 現在の移動入力値
    /// </summary>
    private Vector2 moveInput;
    public Vector2 MoveInput => moveInput;

    /// <summary>
    /// 現在の視点入力値
    /// </summary>
    private Vector2 lookInput;
    public Vector2 LookInput => lookInput;

    /// <summary>
    /// ジャンプ入力要求を保持
    /// </summary>
    private bool jumpRequested;

    /// <summary>
    /// ダッシュ入力要求を保持
    /// </summary>
    private bool dashRequested;

    /// <summary>
    /// ジャンプボタンの押下継続状態
    /// </summary>
    private bool jumpHeld;
    public bool IsJumpHeld => jumpHeld;

    /// <summary>
    /// 移動入力値を更新する
    /// </summary>
    /// <param name="input">入力された移動方向</param>
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    /// <summary>
    /// 視点入力値を更新する
    /// </summary>
    /// <param name="input">入力された視点移動量</param>
    public void SetLookInput(Vector2 input)
    {
        lookInput = input;
    }

    /// <summary>
    /// ダッシュ入力要求を登録する
    /// </summary>
    public void RequestDash()
    {
        dashRequested = true;
    }

    /// <summary>
    /// ジャンプ入力要求を登録し、ジャンプ長押し状態にする
    /// </summary>
    public void PressJump()
    {
        jumpRequested = true;
        jumpHeld = true;
    }

    /// <summary>
    /// ジャンプ長押し状態を解除する
    /// </summary>
    public void ReleaseJump()
    {
        jumpHeld = false;
    }

    /// <summary>
    /// ジャンプ入力要求を1回だけ取り出す
    /// </summary>
    /// <returns>ジャンプ入力要求があった場合はtrue</returns>
    public bool ConsumeJumpRequest()
    {
        if (!jumpRequested)
        {
            return false;
        }

        jumpRequested = false;
        return true;
    }

    /// <summary>
    /// ダッシュ入力要求を1回だけ消費する
    /// </summary>
    /// <returns>ダッシュ入力要求があった場合はtrue</returns>
    public bool ConsumeDashRequest()
    {
        if (!dashRequested)
        {
            return false;
        }

        dashRequested = false;
        return true;
    }

    /// <summary>
    /// プレイヤー操作入力をすべて初期状態へ戻す
    /// </summary>
    public void Reset()
    {
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
        dashRequested = false;
        jumpRequested = false;
        jumpHeld = false;
    }
}
