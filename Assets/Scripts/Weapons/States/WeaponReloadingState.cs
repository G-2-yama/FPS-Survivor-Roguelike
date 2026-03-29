using UnityEngine;

public class WeaponReloadingState : WeaponState
{
    private float timer;

    public WeaponReloadingState(WeaponController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("Weapon Idle Stateに入りました");

        Reload();

        timer = controller.WeaponData.ReloadTime;   // ← WeaponDataの値を使う
    }

    public override void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            controller.WeaponStateMachine.ChangeState(new WeaponCooldownState(controller));
        }
    }

    public override void Exit()
    {
        Debug.Log("Weapon Idle Stateから退出します");
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

    private void Reload()
    {
        // ここでリロード処理を実装
        Debug.Log($"Reloading weapon: {controller.WeaponData.DisplayName}");
    }
}