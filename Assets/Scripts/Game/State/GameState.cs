public abstract class GameState : IState
{
	protected GameController controller;

	public GameState(GameController controller)
	{
		this.controller = controller;
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
