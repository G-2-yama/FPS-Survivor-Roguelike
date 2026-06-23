using UnityEngine;

/// <summary>
/// プレイヤー制御の実処理をまとめるランタイム。
/// MonoBehaviour の入力受信と、ゲーム内データの更新処理を分離するために置いている。
/// </summary>
public class PlayerRuntime
{
    /// <summary>
    /// プレイヤー制御に必要な共有データ
    /// </summary>
    private PlayerContext context;

    /// <summary>
    /// プレイヤー状態更新を管理する
    /// </summary>
    private PlayerStateCoordinator stateCoordinator;

    /// <summary>
    /// 死亡中かどうか
    /// </summary>
    public bool IsDead => stateCoordinator.CurrentBodyStateId == PlayerBodyStateId.Dead;

    /// <summary>
    /// ランタイムを初期化する
    /// </summary>
    public PlayerRuntime(PlayerContext context)
    {
        this.context = context;
        stateCoordinator = new PlayerStateCoordinator(context);
    }

    /// <summary>
    /// 1フレーム分のプレイヤー状態を進める
    /// </summary>
    public void Update(float deltaTime)
    {
        context.JumpController.RefreshGroundState();
        stateCoordinator.Update(deltaTime);
    }

    /// <summary>
    /// プレイヤー死亡時の停止処理をまとめて行う
    /// </summary>
    public void HandleDeath()
    {
        Debug.Log("Playerが死亡しました");

        context.Controls.Reset();
        context.Commands.Reset();
        context.Motor.Stop();
        context.JumpController.Stop();

        stateCoordinator.ChangeToDeadState();
    }

    /// <summary>
    /// 壁面接触時に水平速度を壁へ沿わせる
    /// </summary>
    public void HandleWallHit(Vector3 wallNormal)
    {
        context.Motor.ResolveWallHit(wallNormal);
    }

    public void HandleRecoverableGroundContact()
    {
        context.JumpController.RegisterRecoverableGroundContact();
    }
}
