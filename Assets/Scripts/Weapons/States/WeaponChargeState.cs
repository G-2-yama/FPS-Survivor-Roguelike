using UnityEngine;

public class WeaponChargeState : WeaponState
{
    private float timer;

    public WeaponChargeState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        timer = weapon.WeaponData.ChargeTime;
        weapon.Sounder.Play(SoundCategory.Charge);
    }

    public override void Update(bool isPressed)
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            stateMachine.ChangeFiringState();
        }
    }

    public override void Exit()
    {
        // Debug.Log("Weapon Charge Stateから退出します");
    }

    /// <summary>
	/// 攻撃入力を受け取る
	/// </summary>
	public override void OnFire()
    {
        // Debug.Log($"チャージ中には銃撃はできません");
    }

    /// <summary>
	/// リロード入力を受け取る
	/// </summary>
	public override void OnReload()
    {
        // Debug.Log($"チャージ中にはリロードはできません");
    }

}
