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
        timer = weapon.WeaponData.ReloadTime;
        weapon.Sounder.Play(SoundCategory.ReloadEnter);
    }

    public override void Update(bool isPressed)
    {
        timer -= Time.deltaTime;

        weapon.WeaponView.SetReloadProgress(1f - timer / weapon.WeaponData.ReloadTime);
        // リロードの完了
        if (timer <= 0f)
        {
            weapon.WeaponView.SetReloadProgress(0f);
            weapon.Reload();
            weapon.Sounder.Play(SoundCategory.ReloadEnd);
            stateMachine.ChangeState<WeaponIdleState>();
        }
    }

    public override void Exit()
    {

    }


	public override void OnFire()
    {
    }

	public override void OnReload()
    {
    }
}
