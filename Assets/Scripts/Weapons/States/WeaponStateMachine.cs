using Unity.VisualScripting;

public class WeaponStateMachine : StateMachine<WeaponState>
{
    private WeaponState idleState;
    public WeaponState CurrentWeaponState => currentState;

    private WeaponState firingState;
    public WeaponState FiringState => firingState;

    private WeaponState cooldownState;
    public WeaponState CooldownState => cooldownState;

    private WeaponState reloadingState;
    public WeaponState ReloadingState => reloadingState;


    public WeaponStateMachine(WeaponController controller)
    {
        idleState = new WeaponIdleState(controller);
        firingState = new WeaponFiringState(controller);
        cooldownState = new WeaponCooldownState(controller);
        reloadingState = new WeaponReloadingState(controller);

        ChangeState(idleState);
    }

    public void OnFire()
    {
        currentState?.OnFire();
    }

    public void OnReload()
    {
        currentState?.OnReload();
    }

    /// <summary>
    /// 待機状態に遷移
    /// </summary>
    public void ChangeIdleState()
    {
        ChangeState(idleState);
    }

    /// <summary>
    /// 攻撃状態に遷移
    /// </summary>
    public void ChangeFiringState()
    {
        ChangeState(firingState);
    }

    /// <summary>
    /// クールダウン状態に遷移
    /// </summary>
    public void ChangeCooldownState()
    {
        ChangeState(cooldownState);
    }

    /// <summary>
    /// リロード状態に遷移
    /// </summary>
    public void ChangeReloadingState()
    {
        ChangeState(reloadingState);
    }
}