using UnityEngine;

public class WeaponFiringState : WeaponState
{
    private int burstRemaining;
    private float nextFireTime;
    private bool hasFired;

    public WeaponFiringState(Weapon weapon) : base(weapon)
    {
    }

    public override void Enter()
    {
        burstRemaining = _weapon.WeaponData.BurstCount;
        hasFired = false;
        nextFireTime = stateMachine.ResumeTime ?? Time.time;
        stateMachine.ResumeTime = null;
        stateMachine.BurstStartTime = nextFireTime;
    }

    public override void Update(bool isPressed)
    {
        float interval = _weapon.WeaponData.BurstInterval;

        // 遅れていた射撃を可能な限り消化する
        while (burstRemaining > 0 && Time.time >= nextFireTime)
        {
            // 弾が撃てなかった
            if (!_weapon.Fire())
            {
                EnterCooldown(nextFireTime);
                return;
            }

            // 各弾のモーションを、次の発射までの時間に収める。
            float animationDuration = burstRemaining > 1 ? interval : _weapon.WeaponData.FireInterval;
            _weapon.WeaponView.PlayFireAnimation(animationDuration);

            // 発射音は従来どおりバースト開始時に再生する。
            if (!hasFired)
            {
                _weapon.Sounder.Play(SoundCategory.Fire);
                hasFired = true;
            }

            _weapon.WeaponData.Recoil.AddRecoil();

            burstRemaining--;

            // 本来予定されていた時刻から進める
            nextFireTime += interval;
        }

        if (burstRemaining <= 0)
        {
            EnterCooldown(nextFireTime - interval);
        }
    }

    private void EnterCooldown(float lastShotTime)
    {
        stateMachine.CooldownEndTime = lastShotTime + _weapon.WeaponData.FireInterval;
        stateMachine.ChangeState<WeaponCooldownState>();
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
