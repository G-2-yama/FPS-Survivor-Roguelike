/// <summary>
/// プレイヤーの動作ステートの基底クラス
/// </summary>
public abstract class PlayerActionState : IState
{
    /// <summary>
    /// この動作ステートが通常移動処理を止め、自前で移動を適用するかどうか
    /// </summary>
    public virtual bool BlocksNormalMovement => false;

    /// <summary>
    /// 動作ステートが参照するプレイヤー制御コンテキスト
    /// </summary>
    protected PlayerContext context;

    /// <summary>
    /// 動作ステートの遷移を行うステートマシン
    /// </summary>
    protected PlayerActionStateMachine actionStateMachine;

    /// <summary>
    /// 基底動作ステートを初期化する
    /// </summary>
    /// <param name="context">プレイヤー制御コンテキスト</param>
    /// <param name="actionStateMachine">動作ステートマシン</param>
    public PlayerActionState(
        PlayerContext context,
        PlayerActionStateMachine actionStateMachine)
    {
        this.context = context;
        this.actionStateMachine = actionStateMachine;
    }

    /// <summary>
    /// 状態開始時の初期化処理
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// 状態のフレーム更新処理
    /// </summary>
    public virtual void Update() { }

    /// <summary>
    /// 状態終了時の後処理
    /// </summary>
    public virtual void Exit() { }
}
