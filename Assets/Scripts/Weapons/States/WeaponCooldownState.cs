using UnityEngine;

public class WeaponCooldownState : WeaponState
{
    private float timer;

    public WeaponCooldownState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        timer = _weapon.WeaponData.FireInterval;
    }

    public override void Update(bool isPressed)
    {
        timer -= Time.deltaTime;

        if (timer >= 0f)
        {
            return;
        }

        if (_weapon.WeaponData.TriggerType == WeaponTriggerType.FullAuto && isPressed)
        {
            stateMachine.ChangeState<WeaponFiringState>();
        }
        else
        {
            if (_weapon.WeaponData.AutoReload && _weapon.CurrentAmmo <= 0)
            {
                stateMachine.ChangeState<WeaponReloadingState>();
            }
            else
            {
                stateMachine.ChangeState<WeaponIdleState>();
            }
        }

    }

    public override void Exit()
    {
        // Debug.Log("Weapon Cooldown Stateから退出します");
    }

    /// <summary>
	/// 攻撃入力を受け取る
	/// </summary>
	public override void OnFire()
    {
        // Debug.Log($"クールダウン中には銃撃はできません");
    }

    /// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        // Debug.Log($"クールダウン中にはリロードはできません");
    }

}
