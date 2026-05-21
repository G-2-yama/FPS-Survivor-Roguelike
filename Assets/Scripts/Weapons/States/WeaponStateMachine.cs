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


    public WeaponStateMachine(Weapon weapon)
    {
        idleState = new WeaponIdleState(weapon);
        firingState = new WeaponFiringState(weapon);
        cooldownState = new WeaponCooldownState(weapon);
        reloadingState = new WeaponReloadingState(weapon);

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

    public void OnChangeWeapon(WeaponData data)
    {
        ChangeIdleState();
    }

    public void ChangeIdleState()
    {
        ChangeState(idleState);
    }

    public void ChangeFiringState()
    {
        ChangeState(firingState);
    }

    public void ChangeCooldownState()
    {
        ChangeState(cooldownState);
    }

    public void ChangeReloadingState()
    {
        ChangeState(reloadingState);
    }
}