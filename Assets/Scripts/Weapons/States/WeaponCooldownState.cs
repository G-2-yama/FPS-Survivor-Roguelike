using UnityEngine;

public class WeaponCooldownState : WeaponState
{
    private int enterFrame;

    public WeaponCooldownState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        enterFrame = Time.frameCount;
    }

    public override void Update(bool isPressed)
    {
        float endTime = stateMachine.CooldownEndTime;
        if (Time.time < endTime)
        {
            return;
        }

        bool keepFiring = _weapon.WeaponData.TriggerType == WeaponTriggerType.FullAuto && isPressed;
        if (!keepFiring && _weapon.WeaponData.AutoReload && _weapon.CurrentAmmo <= 0)
        {
            stateMachine.ChangeState<WeaponReloadingState>();
            return;
        }

        if (keepFiring || _weapon.WeaponData.AutoFire)
        {
            if (Time.frameCount == enterFrame && endTime <= stateMachine.BurstStartTime)
            {
                return;
            }
            stateMachine.ResumeTime = endTime;
        }

        if (keepFiring)
        {
            stateMachine.ChangeState<WeaponFiringState>();
        }
        else
        {
            stateMachine.ChangeState<WeaponIdleState>();
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
