using UnityEngine;

public class WeaponFiringState : WeaponState
{
    public WeaponFiringState(Weapon weapon) : base(weapon) { }

    private int burstRemaining;
    private float timer;
    private bool hasFired;

    public override void Enter()
    {
        // Debug.Log("Weapon Firing Stateに入りました");

        burstRemaining = weapon.WeaponData.BurstCount;
        hasFired = false;
        timer = 0f;
    }

    public override void Update(bool isPressed)
    {
        // 全弾撃ち終わった場合
        if (burstRemaining <= 0)
        {
            stateMachine.ChangeCooldownState();
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        if (weapon.Fire())
        {
            if (!hasFired)
            {
                weapon.WeaponView.PlayFireAnimation();
                weapon.Sounder.Play(SoundCategory.Fire);
                hasFired = true;
            }
            weapon.WeaponData.Recoil.AddRecoil();
            burstRemaining--;
            timer = weapon.WeaponData.BurstInterval;
        }
        else
        {
            stateMachine.ChangeCooldownState();
        }
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
        // Debug.Log($"銃撃中に再度銃撃はできません");
    }

	/// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        // Debug.Log($"銃撃中にリロードはできません");
    }
}
