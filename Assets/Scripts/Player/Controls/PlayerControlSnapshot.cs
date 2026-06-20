using UnityEngine;

/// <summary>
/// 継続して参照するプレイヤー操作の現在値を保持する。
/// 移動・視点・押下継続のような「今どう入力されているか」だけを扱う。
/// </summary>
public class PlayerControlState
{
    /// <summary>
    /// 現在の移動入力値
    /// </summary>
    public Vector2 MoveInput { get; private set; }

    /// <summary>
    /// 現在の視点入力値
    /// </summary>
    public Vector2 LookInput { get; private set; }

    /// <summary>
    /// ジャンプボタンを押し続けているかどうか
    /// </summary>
    public bool JumpHeld { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool CrouchHeld { get; private set; }

    /// <summary>
    /// 移動入力値を更新する
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        MoveInput = input;
    }

    /// <summary>
    /// 視点入力値を更新する
    /// </summary>
    public void SetLookInput(Vector2 input)
    {
        LookInput = input;
    }

    /// <summary>
    /// ジャンプ押下継続状態を更新する
    /// </summary>
    public void SetJumpHeld(bool isHeld)
    {
        JumpHeld = isHeld;
    }

    public void SetSprintHeld(bool isHeld)
    {
        SprintHeld = isHeld;
    }

    public void SetCrouchHeld(bool isHeld)
    {
        CrouchHeld = isHeld;
    }

    /// <summary>
    /// 継続入力を初期状態へ戻す
    /// </summary>
    public void Reset()
    {
        MoveInput = Vector2.zero;
        LookInput = Vector2.zero;
        JumpHeld = false;
        SprintHeld = false;
        CrouchHeld = false;
    }
}
