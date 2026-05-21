using UnityEngine;

public class WeaponReloadingState : WeaponState
{
    /// <summary>
    /// リロードのタイマー
    /// </summary>
    private float timer;

    public WeaponReloadingState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        Debug.Log("Weapon Reloading Stateに入りました");
        weapon.WeaponView.PlayReloadAnimation();
        timer = weapon.WeaponStats.ReloadTime;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;

        weapon.WeaponView.SetReloadProgress(1f - timer / weapon.WeaponStats.ReloadTime);
        // リロードの完了
        if (timer <= 0f)
        {
            weapon.WeaponView.SetReloadProgress(0f);
            weapon.Reload();
            stateMachine.ChangeIdleState();
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
