using UnityEngine;

public class WeaponReloadingState : WeaponState
{
    /// <summary>
    /// リロードのタイマー
    /// </summary>
    private float timer;

    public WeaponReloadingState(WeaponController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Weapon Reloading Stateに入りました");
        controller.WeaponView.PlayReloadAnimation();
        timer = controller.Weapon.WeaponStats.ReloadTime;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;

        controller.WeaponView.SetReloadProgress(1f - timer / controller.Weapon.WeaponStats.ReloadTime);
        // リロードの完了
        if (timer <= 0f)
        {
            controller.WeaponView.SetReloadProgress(0f);
            controller.Weapon.Reload();
            controller.WeaponStateMachine.ChangeIdleState();
        }
    }

    public override void Exit()
    {
        // Debug.Log("Weapon Reloading Stateから退出します");
    }

    /// <summary>
	/// 攻撃入力を受け取る
	/// </summary>
	public override void OnFire()
    {
        Debug.Log("リロード中には攻撃できません");
    }

	/// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        Debug.Log("リロード中には再度リロードできません");
    }
}
