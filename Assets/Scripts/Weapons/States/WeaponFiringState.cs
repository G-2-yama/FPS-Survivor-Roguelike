using UnityEngine;

public class WeaponFiringState : WeaponState
{
    public WeaponFiringState(WeaponController controller) : base(controller) { }
    
    private int burstRemaining;
    private float timer;
    private bool hasFired;

    public override void Enter()
    {
        Debug.Log("Weapon Firing Stateに入りました");

        burstRemaining = controller.Weapon.WeaponStats.BurstCount;
        timer = 0f;
    }

    public override void Update()
    {
        // 全弾撃ち終わった場合
        if (burstRemaining <= 0)
        {
            TransitionAfterBurst();
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (controller.Weapon.Fire())
        {
            controller.WeaponView.PlayFireAnimation();
            controller.WeaponRecoil.AddRecoil();
            burstRemaining--;
            timer = controller.Weapon.WeaponStats.BurstInterval;
        }
        else
        {
            TransitionAfterBurst();
        }
    }

    private void TransitionAfterBurst()
    {
        if (controller.Weapon.ShouldStartAutoReload())
        {
            controller.WeaponStateMachine.ChangeReloadingState();
            return;
        }

        controller.WeaponStateMachine.ChangeCooldownState();
    }

    public override void Exit()
    {
        // Debug.Log("Weapon Firing Stateから退出します");
    }

    /// <summary>
	/// 攻撃入力を受け取る
	/// </summary>
	public override void OnFire()
    {
        Debug.Log($"銃撃中に再度銃撃はできません");
    }

	/// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        Debug.Log($"銃撃中にリロードはできません");
    }
}
