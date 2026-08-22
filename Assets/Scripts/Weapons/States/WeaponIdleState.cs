using System;
using UnityEngine;

public class WeaponIdleState : WeaponState
{
    public WeaponIdleState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        if (!_weapon.HasWeapon)
        {
            return;
        }

        _weapon.WeaponView.SetReloadProgress(0f);


        if (_weapon.WeaponData.AutoFire)
        {
            stateMachine.ChangeState<WeaponFiringState>();
        }
    }

    public override void Update(bool isPressed)
    {

    }

    public override void Exit()
    {

    }

    /// <summary>
	/// 攻撃入力を受け取る
	/// </summary>
	public override void OnFire()
    {
        stateMachine.ChangeState<WeaponFiringState>();
    }

	/// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        stateMachine.ChangeState<WeaponReloadingState>();
    }
}