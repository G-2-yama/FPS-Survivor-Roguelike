using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    public Weapon Weapon => weapon;

    /// <summary>
    /// 攻撃入力が押されているかどうかを管理するフラグ
    /// </summary>
    private bool isFirePressed;

    public bool IsFirePressed => isFirePressed;

    private bool isInputEnabled = true;
    public bool IsInputEnabled => isInputEnabled;

    /// <summary>
    /// 初期化
    /// </summary>
    void Start()
    {
        WeaponControllerManager.OnGlobalFire += OnGlobalFireReceived;
        WeaponControllerManager.OnGlobalFireReleased += OnGlobalFireReleasedReceived;
        WeaponControllerManager.OnReload += OnReloadReceived;
    }

    private void OnDestroy()
    {
        WeaponControllerManager.OnGlobalFire -= OnGlobalFireReceived;
        WeaponControllerManager.OnGlobalFireReleased -= OnGlobalFireReleasedReceived;
        WeaponControllerManager.OnReload -= OnReloadReceived;
    }

    public void Update()
    {
        weapon.StateMachine.Update(isFirePressed);
    }

    public void EnableInput()
    {
        isInputEnabled = true;
    }

    public void DisableInput()
    {
        isInputEnabled = false;
    }

    /// <summary>
    /// 攻撃入力を処理するメソッド
    /// </summary>
    public void OnFire(InputAction.CallbackContext context)
    {
        if (!weapon.HasWeapon || !IsInputEnabled)
        {
            return;
        }

        if (context.phase == InputActionPhase.Started)
        {
            isFirePressed = true;
            weapon.StateMachine.OnFire();
            WeaponControllerManager.BroadcastFire();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isFirePressed = false;
            WeaponControllerManager.BroadcastFireReleased();
        }
    }

    /// <summary>
    /// リロード入力を処理するメソッド
    /// </summary>
    public void OnReload(InputAction.CallbackContext context)
    {
        if (!weapon.HasWeapon || !IsInputEnabled)
        {
            return;
        }

        if (context.phase == InputActionPhase.Performed)
        {
            weapon.StateMachine.OnReload();
            WeaponControllerManager.BroadcastReload();
        }
    }

    private void OnGlobalFireReceived()
    {
        if (isFirePressed) return;
        if (!weapon.HasWeapon || !IsInputEnabled) return;

        isFirePressed = true;
        weapon.StateMachine.OnFire();
    }

    private void OnGlobalFireReleasedReceived()
    {
        if (!isFirePressed) return;
        isFirePressed = false;
    }

    private void OnReloadReceived()
    {
        if (!weapon.HasWeapon || !IsInputEnabled) return;

        weapon.StateMachine.OnReload();
    }
}