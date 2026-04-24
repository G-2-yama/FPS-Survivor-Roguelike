/// <summary>
/// プレイヤー制御で共有するデータ参照をまとめる。
/// 各システムはこのコンテキストだけを受け取り、MonoBehaviour へ直接依存しない。
/// </summary>
public class PlayerContext
{
    /// <summary>
    /// プレイヤー体力モデル
    /// </summary>
    public Player Player { get; }

    /// <summary>
    /// プレイヤー全体設定
    /// </summary>
    public PlayerConfig Config { get; }

    /// <summary>
    /// 武器の入力・攻撃処理を管理するコントローラー
    /// </summary>
    public WeaponController WeaponController { get; }

    /// <summary>
    /// CharacterControllerを使った移動計算
    /// </summary>
    public PlayerMotor Motor { get; }

    /// <summary>
    /// 歩行や空中移動の速度更新
    /// </summary>
    public PlayerLocomotion Locomotion { get; }

    /// <summary>
    /// ジャンプ回数とジャンプ補正
    /// </summary>
    public PlayerJumpController JumpController { get; }

    /// <summary>
    /// 視点制御
    /// </summary>
    public PlayerLookController Look { get; }

    /// <summary>
    /// 継続入力の現在値
    /// </summary>
    public PlayerControlState Controls { get; }

    /// <summary>
    /// 単発入力要求のバッファ
    /// </summary>
    public PlayerCommandBuffer Commands { get; }

    /// <summary>
    /// プレイヤー制御に必要な参照を初期化する
    /// </summary>
    public PlayerContext(
        Player player,
        PlayerConfig config,
        WeaponController weaponController,
        PlayerMotor motor,
        PlayerLocomotion locomotion,
        PlayerJumpController jumpController,
        PlayerLookController look,
        PlayerControlState controls,
        PlayerCommandBuffer commands)
    {
        Player = player;
        Config = config;
        WeaponController = weaponController;
        Motor = motor;
        Locomotion = locomotion;
        JumpController = jumpController;
        Look = look;
        Controls = controls;
        Commands = commands;
    }

    /// <summary>
    /// 残りジャンプ回数があればジャンプを開始する
    /// </summary>
    public bool TryJump()
    {
        return JumpController.TryJump();
    }
}
