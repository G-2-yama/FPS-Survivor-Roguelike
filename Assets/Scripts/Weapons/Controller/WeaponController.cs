using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon weapon;
    [SerializeField] private WeaponControllerManager manager;

    public Weapon Weapon => weapon;
    public bool IsFirePressed { get; private set; }
    public bool CanControlWeapon() => weapon.HasWeapon && !weapon.WeaponData.IsEmpty;

    private void Start()
    {
        manager.Register(this);
    }

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.Unregister(this);
        }
    }

    private void Update()
    {
        weapon.StateMachine.Update(IsFirePressed);
    }

    /// <summary>
    /// 武器の発射入力を受け取る
    /// </summary>
    /// <param name="context">入力情報</param>
    public void OnFire(InputAction.CallbackContext context)
    {
        if (!CanControlWeapon())
            return;

        // Fireボタンが押されたときの処理
        if (context.started)
        {
            Fire();
            manager.OnWeaponFired(this);
        }
        else if (context.canceled)
        {
            Release();
            manager.OnWeaponReleased(this);
        }
    }

    /// <summary>
    /// 武器のリロード入力を受け取る
    /// </summary>
    /// <param name="context">入力情報</param>
    public void OnReload(InputAction.CallbackContext context)
    {
        if (!CanControlWeapon())
            return;

        if (context.performed)
        {
            Reload();
            manager.OnWeaponReloaded(this);
        }
    }

    public void FireSync()
    {
        if (!CanControlWeapon())
            return;

        Fire();
    }

    public void ReleaseSync()
    {
        if (!IsFirePressed)
            return;

        Release();
    }

    public void ReloadSync()
    {
        if (!CanControlWeapon())
            return;

        Reload();
    }


    private void Fire()
    {
        IsFirePressed = true;
        weapon.StateMachine.OnFire();
    }

    private void Release()
    {
        IsFirePressed = false;
    }

    private void Reload()
    {
        weapon.StateMachine.OnReload();
    }
}