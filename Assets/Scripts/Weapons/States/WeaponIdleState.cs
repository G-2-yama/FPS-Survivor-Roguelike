using UnityEngine;

public class WeaponIdleState : WeaponState
{
    public WeaponIdleState(WeaponController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Weapon Idle Stateに入りました");
    }

    public override void Update()
    {
        // 待機状態の更新処理をここに実装
    }

    public override void Exit()
    {
        Debug.Log("Weapon Idle Stateから退出します");
    }

    /// <summary>
	/// 攻撃入力を受け取る
	/// </summary>
	public override void OnFire()
    {
        controller.WeaponStateMachine.ChangeState(new WeaponFiringState(controller));
    }

	/// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        controller.WeaponStateMachine.ChangeState(new WeaponReloadingState(controller));
    }
}