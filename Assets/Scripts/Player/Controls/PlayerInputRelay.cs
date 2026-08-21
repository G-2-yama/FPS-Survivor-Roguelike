using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Unity の入力コールバックをプレイヤー制御用データへ中継する。
/// MonoBehaviour 側は受信だけを担当し、入力による状態変更はこのクラスへ寄せる。
/// </summary>
public class PlayerInputRelay
{
    /// <summary>
    /// 継続入力の反映先
    /// </summary>
    private PlayerControlState controls;

    /// <summary>
    /// 単発入力要求の反映先
    /// </summary>
    private PlayerCommandBuffer commands;

    /// <summary>
    /// 射撃・リロード入力を中継する武器コントローラー
    /// </summary>
    private WeaponControllerManager weaponControllerManager;

    /// <summary>
    /// ゲームプレイ入力を受け付けてよい状態か判定する処理
    /// </summary>
    private Func<bool> canHandleGameplayInput;

    /// <summary>
    /// 入力中継に必要な参照を初期化する
    /// </summary>
    public PlayerInputRelay(
        PlayerControlState controls,
        PlayerCommandBuffer commands,
        WeaponControllerManager weaponControllerManager,
        Func<bool> canHandleGameplayInput)
    {
        this.controls = controls;
        this.commands = commands;
        this.weaponControllerManager = weaponControllerManager;
        this.canHandleGameplayInput = canHandleGameplayInput;
    }

    /// <summary>
    /// 移動入力を継続入力へ反映する
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        controls.SetMoveInput(context.ReadValue<Vector2>());
    }

    /// <summary>
    /// ダッシュ入力を単発要求として登録する
    /// </summary>
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            controls.SetSprintHeld(true);
            commands.EnqueueDash();
        }

        if (context.canceled)
        {
            controls.SetSprintHeld(false);
        }
    }

    /// <summary>
    /// 視点入力を継続入力へ反映する
    /// </summary>
    public void OnLook(InputAction.CallbackContext context)
    {
        bool isGamepadInput = context.control?.device is Gamepad;
        controls.SetLookInput(context.ReadValue<Vector2>(), isGamepadInput);
    }

    /// <summary>
    /// ジャンプ入力を継続入力と単発要求へ反映する
    /// </summary>
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            controls.SetJumpHeld(true);
            commands.EnqueueJump();
        }

        if (context.canceled)
        {
            controls.SetJumpHeld(false);
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            controls.SetCrouchHeld(true);
            commands.EnqueueCrouchAction();
        }

        if (context.canceled)
        {
            controls.SetCrouchHeld(false);
            commands.DisarmSlideOnNextLand();
        }
    }

}
