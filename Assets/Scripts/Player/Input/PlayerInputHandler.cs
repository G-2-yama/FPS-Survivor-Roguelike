using System;
using UnityEngine.InputSystem;

/// <summary>
/// Input Systemのコールバックを入力状態更新と武器操作へ振り分けるハンドラー
/// </summary>
public class PlayerInputHandler
{
    /// <summary>
    /// 更新対象の入力状態モデル
    /// </summary>
    private PlayerInputState inputState;

    /// <summary>
    /// 射撃・リロード入力を中継する武器コントローラー
    /// </summary>
    private WeaponController weaponController;

    /// <summary>
    /// ゲームプレイ入力を受け付けてよいかを判定する処理
    /// </summary>
    private Func<bool> canHandleGameplayInput;

    /// <summary>
    /// 入力処理に必要な参照を初期化する
    /// </summary>
    /// <param name="inputState">更新対象の入力状態モデル</param>
    /// <param name="weaponController">武器入力の中継先</param>
    /// <param name="canHandleGameplayInput">ゲームプレイ入力を受け付けてよい場合にtrueを返す処理</param>
    public PlayerInputHandler(
        PlayerInputState inputState,
        WeaponController weaponController,
        Func<bool> canHandleGameplayInput)
    {
        this.inputState = inputState;
        this.weaponController = weaponController;
        this.canHandleGameplayInput = canHandleGameplayInput;
    }

    /// <summary>
    /// 移動入力を受け取り、入力状態へ反映する
    /// </summary>
    /// <param name="context">Input Systemの入力コンテキスト</param>
    public void OnMove(InputAction.CallbackContext context)
    {
        inputState.SetMoveInput(context.ReadValue<UnityEngine.Vector2>());
    }

    /// <summary>
    /// ダッシュ入力を受け取り、ダッシュ要求を登録する
    /// </summary>
    /// <param name="context">Input Systemの入力コンテキスト</param>
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inputState.RequestDash();
        }
    }

    /// <summary>
    /// 視点入力を受け取り、入力状態へ反映する
    /// </summary>
    /// <param name="context">Input Systemの入力コンテキスト</param>
    public void OnLook(InputAction.CallbackContext context)
    {
        inputState.SetLookInput(context.ReadValue<UnityEngine.Vector2>());
    }

    /// <summary>
    /// ジャンプ入力を受け取り、入力要求と長押し状態へ反映する
    /// </summary>
    /// <param name="context">Input Systemの入力コンテキスト</param>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inputState.PressJump();
        }

        if (context.canceled)
        {
            inputState.ReleaseJump();
        }
    }

    /// <summary>
    /// 射撃入力を武器コントローラーへ中継する
    /// </summary>
    /// <param name="context">Input Systemの入力コンテキスト</param>
    public void OnFire(InputAction.CallbackContext context)
    {
        if (canHandleGameplayInput())
        {
            weaponController.OnFire(context);
        }
    }

    /// <summary>
    /// リロード入力を武器コントローラーへ中継する
    /// </summary>
    /// <param name="context">Input Systemの入力コンテキスト</param>
    public void OnReload(InputAction.CallbackContext context)
    {
        if (canHandleGameplayInput())
        {
            weaponController.OnReload(context);
        }
    }
}
