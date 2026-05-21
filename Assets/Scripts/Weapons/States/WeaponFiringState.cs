using UnityEngine;

public class WeaponFiringState : WeaponState
{
    public WeaponFiringState(Weapon weapon) : base(weapon) { }

    private int burstRemaining;
    private float timer;
    private bool hasFired;

    public override void Enter()
    {
        Debug.Log("Weapon Firing Stateに入りました");

        burstRemaining = weapon.WeaponStats.BurstCount;
        hasFired = false;
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

        if (weapon.Fire())
        {
            if (!hasFired)
            {
                weapon.WeaponView.PlayFireAnimation();
                hasFired = true;
            }
            weapon.WeaponRecoil.AddRecoil();
            burstRemaining--;
            timer = weapon.WeaponStats.BurstInterval;
        }
        else
        {
            TransitionAfterBurst();
        }
    }

    private void TransitionAfterBurst()
    {
        if (weapon.ShouldStartAutoReload())
        {
            stateMachine.ChangeReloadingState();
            return;
        }

        stateMachine.ChangeCooldownState();
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
