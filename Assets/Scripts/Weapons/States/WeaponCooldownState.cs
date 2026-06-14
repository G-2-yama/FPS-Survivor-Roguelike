using UnityEngine;

public class WeaponCooldownState : WeaponState
{
    private float timer;

    public WeaponCooldownState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        // Debug.Log("Weapon Cooldown Stateに入りました");
        timer = weapon.WeaponData.FireInterval;
    }

    public override void Update(bool isPressed)
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (weapon.WeaponData.TriggerType == WeaponTriggerType.FullAuto && isPressed)
            {
                stateMachine.ChangeFiringState();
            }
            else
            {
                Transition();
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

    private void Transition()
    {
        if (weapon.ShouldStartAutoReload())
        {
            stateMachine.ChangeReloadingState();
            return;
        }

        stateMachine.ChangeIdleState();
    }

}
