using UnityEngine;

public class WeaponChargeState : WeaponState
{
    private float timer;

    public WeaponChargeState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        timer = weapon.WeaponData.ChargeTime;
        weapon.Sounder.Play(SoundCategory.Charge);
    }

    public override void Update(bool isPressed)
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            stateMachine.ChangeState<WeaponFiringState>();
        }
    }

    public override void Exit()
    {

    }

	public override void OnFire()
    {
    }

	public override void OnReload()
    {
    }

}
