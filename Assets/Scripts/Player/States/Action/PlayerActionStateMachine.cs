using UnityEngine;

/// <summary>
/// プレイヤーの動作ステートを管理するステートマシン
/// </summary>
public class PlayerActionStateMachine : StateMachine<PlayerActionState>
{
    /// <summary>
    /// 特殊動作を行っていない状態
    /// </summary>
    private PlayerActionState noActionState;
    public PlayerActionState NoActionState => noActionState;

    /// <summary>
    /// ダッシュ動作状態
    /// </summary>
    private PlayerDashActionState dashState;
    public PlayerDashActionState DashState => dashState;

    /// <summary>
    /// 現在の動作ステート
    /// </summary>
    public PlayerActionState CurrentActionState => currentState;

    /// <summary>
    /// 通常移動を止めて、動作ステート側で移動を適用する状態かどうか
    /// </summary>
    public bool IsBlockingNormalMovement => currentState?.BlocksNormalMovement ?? false;

    /// <summary>
    /// 動作ステートを生成し、特殊動作なし状態へ遷移する
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト</param>
    public PlayerActionStateMachine(PlayerContext context)
    {
        noActionState = new PlayerNoActionState(context, this);
        dashState = new PlayerDashActionState(context, this);

        ChangeState(noActionState);
    }

    /// <summary>
    /// 現在の動作状態を更新する前にダッシュのクールタイムを更新する
    /// </summary>
    public new void Update()
    {
        dashState.UpdateCooldown(Time.deltaTime);
        base.Update();
    }

    /// <summary>
    /// 何もしていない動作状態に遷移
    /// </summary>
    public void ChangeNoActionState()
    {
        ChangeState(noActionState);
    }

    /// <summary>
    /// ダッシュ開始条件を満たしている場合にダッシュ動作へ遷移する
    /// </summary>
    /// <returns>ダッシュ動作へ遷移した場合はtrue</returns>
    public bool TryChangeDashState()
    {
        if (!dashState.CanEnter())
        {
            return false;
        }

        ChangeState(dashState);
        return true;
    }
}
