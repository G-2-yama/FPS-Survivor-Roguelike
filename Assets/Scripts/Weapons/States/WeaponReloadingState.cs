using UnityEngine;

public class WeaponReloadingState : WeaponState
{
    /// <summary>
    /// リロードのタイマー
    /// </summary>
    private float _timer;
    private float _duration;

    public WeaponReloadingState(Weapon weapon) : base(weapon) { }

    public override void Enter()
    {
        _duration = Mathf.Max(_weapon.WeaponData.ReloadTime, 0.01f);
        _timer = _duration;
        _weapon.WeaponView.PlayReloadAnimation(_duration);
        _weapon.Sounder.Play(SoundCategory.ReloadEnter);
    }

    public override void Update(bool isPressed)
    {
        _timer -= Time.deltaTime;

        _weapon.WeaponView.SetReloadProgress(Mathf.Clamp01(1f - _timer / _duration));
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
