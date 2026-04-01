using UnityEngine;

public class WeaponCooldownState : WeaponState
{
    private float timer;

    public WeaponCooldownState(WeaponController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Weapon Cooldown Stateに入りました");
        timer = controller.Weapon.WeaponData.FireInterval;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            controller.WeaponStateMachine.ChangeState(
                new WeaponIdleState(controller)
            );
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
