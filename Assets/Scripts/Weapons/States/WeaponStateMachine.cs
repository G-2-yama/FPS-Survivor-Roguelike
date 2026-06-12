using Unity.VisualScripting;

public class WeaponStateMachine
{
    public WeaponState currentState;
    private WeaponState idleState;

    private WeaponState firingState;

    private WeaponState cooldownState;

    private WeaponState reloadingState;
    private WeaponState chargeState;


    public WeaponStateMachine(Weapon weapon)
    {
        idleState = new WeaponIdleState(weapon);
        firingState = new WeaponFiringState(weapon);
        cooldownState = new WeaponCooldownState(weapon);
        reloadingState = new WeaponReloadingState(weapon);
        chargeState = new WeaponChargeState(weapon);

        ChangeState(idleState);
    }

    public void Update(bool isPressed)
    {
        currentState?.Update(isPressed);
    }

    public void OnFire()
    {
        currentState?.OnFire();
    }

    public void OnReload()
    {
        currentState?.OnReload();
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

    public void ChangeChargeState()
    {
        ChangeState(chargeState);
    }

    private void ChangeState(WeaponState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
}