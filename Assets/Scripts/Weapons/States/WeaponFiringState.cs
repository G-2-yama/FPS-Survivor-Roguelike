using UnityEngine;

public class WeaponFiringState : WeaponState
{
    public WeaponFiringState(WeaponController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Weapon Firing Stateに入りました");

        controller.Weapon.Fire();
        controller.WeaponRecoil.AddRecoil();

        // 発射後はクールタイムへ
        controller.WeaponStateMachine.ChangeState(new WeaponCooldownState(controller));
    }

    public override void Update()
    {
        // 発射状態の更新処理をここに実装
    }

    public override void Exit()
    {
        // Debug.Log("Weapon Firing Stateから退出します");
    }

    /// <summary>
	/// 攻撃入力を受け取る
	/// </summary>
	public override void OnFire()
    {
        Debug.Log($"銃撃中に再度銃撃はできません");
    }

	/// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        Debug.Log($"銃撃中にリロードはできません");
    }
}
