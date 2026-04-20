public abstract class WeaponState : IState
{
	protected WeaponController controller;

	public WeaponState(WeaponController controller)
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

	/// <summary>
	/// 攻撃入力を受け取る
	/// </summary>
	public virtual void OnFire() { }

	/// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public virtual void OnReload() { }

	/// <summary>
	/// 武器装備が変更されたときに呼ばれる
	/// </summary>
	public virtual void OnChangeWeapon(WeaponData data) { }
}
