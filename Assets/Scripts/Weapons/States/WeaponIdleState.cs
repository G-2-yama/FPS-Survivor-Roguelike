using UnityEngine;

public class WeaponIdleState : WeaponState
{
    public WeaponIdleState(WeaponController controller) : base(controller) { }

    public override void Enter()
    {
        controller.WeaponView.SetReloadProgress(0f);

        if(controller.Weapon.WeaponData == null)
        {
            return;
        }

        if (controller.Weapon.WeaponData.AutoReload && controller.Weapon.CurrentAmmo <= 0)
        {
            controller.WeaponStateMachine.ChangeReloadingState();
        }
    }

    public override void Update()
    {
        // 待機状態の更新処理をここに実装
    }

    public override void Exit()
    {
        // Debug.Log("Weapon Idle Stateから退出します");
    }

    /// <summary>
	/// 攻撃入力を受け取る
	/// </summary>
	public override void OnFire()
    {
        controller.WeaponStateMachine.ChangeFiringState();
    }

	/// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        controller.WeaponStateMachine.ChangeReloadingState();
    }

    public override void OnChangeWeapon(WeaponData data)
    {
        controller.WeaponView.SetReloadProgress(0f);
        controller.WeaponStateMachine.ChangeIdleState();
    }
}