using UnityEngine;

/// <summary>
/// プレイヤー制御に必要なモデル・入力・移動・視点への参照をまとめるコンテキスト
/// </summary>
public class PlayerContext
{
    /// <summary>
    /// 体力と移動設定を保持するプレイヤーモデル
    /// </summary>
    public PlayerHealth Player { get; }

    /// <summary>
    /// 武器の入力・攻撃処理を管理するコントローラー
    /// </summary>
    public WeaponController WeaponController { get; }

    /// <summary>
    /// CharacterControllerを使った移動計算
    /// </summary>
    public PlayerMotor Motor { get; }

    /// <summary>
    /// プレイヤー本体とカメラピッチの視点制御
    /// </summary>
    public PlayerLookController Look { get; }

    /// <summary>
    /// 現在の入力状態
    /// </summary>
    public PlayerInputState Input { get; }

    /// <summary>
    /// 接地状態を保持
    /// </summary>
    public bool IsGrounded { get; private set; }

    /// <summary>
    /// プレイヤー制御に必要な参照を初期化する
    /// </summary>
    /// <param name="player">プレイヤーモデル</param>
    /// <param name="weaponController">武器コントローラー</param>
    /// <param name="motor">移動処理</param>
    /// <param name="look">視点処理</param>
    /// <param name="input">入力状態</param>
    public PlayerContext(
        PlayerHealth player,
        WeaponController weaponController,
        PlayerMotor motor,
        PlayerLookController look,
        PlayerInputState input)
    {
        Player = player;
        WeaponController = weaponController;
        Motor = motor;
        Look = look;
        Input = input;
    }

    /// <summary>
    /// 接地状態を更新する
    /// </summary>
    /// <param name="isGrounded">現在接地している場合はtrue</param>
    public void SetGrounded(bool isGrounded)
    {
        IsGrounded = isGrounded;
    }

    /// <summary>
    /// 残りジャンプ回数があればジャンプを開始する
    /// </summary>
    /// <returns>ジャンプを開始できた場合はtrue</returns>
    public bool TryJump()
    {
        return Motor.TryJump();
    }
}
