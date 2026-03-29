using Unity.VisualScripting;

public class WeaponStateMachine : StateMachine<WeaponState>
{
    public void OnFire()
    {
        currentState?.OnFire();
    }

    public void OnReload()
    {
        currentState?.OnReload();
    }
}