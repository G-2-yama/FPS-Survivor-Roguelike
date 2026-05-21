using UnityEngine;

public class WeaponCooldownState : WeaponState
{
    private float timer;

    public WeaponCooldownState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        Debug.Log("Weapon Cooldown Stateに入りました");
        timer = weapon.WeaponStats.FireInterval;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            if (weapon.WeaponData.TriggerType == WeaponTriggerType.FullAuto)
            {
                stateMachine.ChangeFiringState();
            }
            else
            {
                stateMachine.ChangeIdleState();
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
        Debug.Log($"クールダウン中には銃撃はできません");
    }

    /// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        Debug.Log($"クールダウン中にはリロードはできません");
    }

}
