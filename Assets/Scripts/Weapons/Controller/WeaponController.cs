// WeaponController.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    public Weapon Weapon => weapon;

    private bool isFirePressed;
    public bool IsFirePressed => isFirePressed;

    private bool isInputEnabled = true;
    public bool IsInputEnabled => isInputEnabled;

    [SerializeField] private WeaponControllerManager manager;

    void Start()
    {
        manager.Register(this);
    }

    private void OnDestroy()
    {
        manager.Unregister(this);
    }

    public void Update()
    {
        weapon.StateMachine.Update(isFirePressed);
    }

    public void EnableInput()  => isInputEnabled = true;
    public void DisableInput() => isInputEnabled = false;


    public void OnFire(InputAction.CallbackContext context)
    {
        if (!weapon.HasWeapon || !isInputEnabled) return;

        if (context.phase == InputActionPhase.Started)
        {
            FireInternal();
            manager.OnWeaponFired(this);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            ReleaseInternal();
            manager.OnWeaponReleased(this);
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (!weapon.HasWeapon || !isInputEnabled) return;

        if (context.phase == InputActionPhase.Performed)
        {
            ReloadInternal();
            manager.OnWeaponReloaded(this);
        }
    }

    // ────── Manager から呼ばれる同期メソッド ──────
    // publicだが、直接Inputからは呼ばれない

    public void FireSync()
    {
        if (!weapon.HasWeapon || !isInputEnabled || isFirePressed) return;
        FireInternal();
    }

    public void ReleaseSync()
    {
        if (!isFirePressed) return;
        ReleaseInternal();
    }

    public void ReloadSync()
    {
        if (!weapon.HasWeapon || !isInputEnabled) return;
        ReloadInternal();
    }


    private void FireInternal()
    {
        isFirePressed = true;
        weapon.StateMachine.OnFire();
    }

    private void ReleaseInternal()
    {
        isFirePressed = false;
    }

    private void ReloadInternal()
    {
        weapon.StateMachine.OnReload();
    }
}