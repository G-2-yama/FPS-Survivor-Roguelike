using UnityEngine;

public class WeaponReloadingState : WeaponState
{
    /// <summary>
    /// リロードのタイマー
    /// </summary>
    private float _timer;

    public WeaponReloadingState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        Debug.Log("Weapon Reloading Stateに入りました");
        _weapon.WeaponView.PlayReloadAnimation();
        _timer = _weapon.WeaponData.ReloadTime;
        _weapon.Sounder.Play(SoundCategory.ReloadEnter);
    }

    public override void Update(bool isPressed)
    {
        _timer -= Time.deltaTime;

        _weapon.WeaponView.SetReloadProgress(1f - _timer / _weapon.WeaponData.ReloadTime);
        // リロードの完了
        if (_timer <= 0f)
        {
            _weapon.WeaponView.SetReloadProgress(0f);
            _weapon.Reload();
            _weapon.Sounder.Play(SoundCategory.ReloadEnd);
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
