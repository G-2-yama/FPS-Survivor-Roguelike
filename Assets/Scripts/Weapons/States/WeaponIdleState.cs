using System;
using UnityEngine;

public class WeaponIdleState : WeaponState
{
    public WeaponIdleState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        if (weapon.WeaponData == null)
        {
            return;
        }

        weapon.WeaponView.SetReloadProgress(0f);

        if (weapon.ShouldStartAutoReload())
        {
            stateMachine.ChangeReloadingState();
        }
        else if (weapon.WeaponData.AutoFire)
        {
            stateMachine.ChangeFiringState();
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
        stateMachine.ChangeFiringState();
    }

	/// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        stateMachine.ChangeReloadingState();
    }
}