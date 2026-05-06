using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    public Weapon Weapon => weapon;
    
    [SerializeField] private WeaponView weaponView;
    public WeaponView WeaponView => weaponView;

    private WeaponStateMachine weaponStateMachine;
    public WeaponStateMachine WeaponStateMachine => weaponStateMachine;

    /// <summary>
    /// 武器の反動を管理するクラス
    /// </summary>
    private WeaponRecoil weaponRecoil;
    public WeaponRecoil WeaponRecoil => weaponRecoil;

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
    void Awake()
    {
        weaponStateMachine = new WeaponStateMachine(this);
        weaponRecoil = new WeaponRecoil(weapon);

        weapon.OnWeaponEquipped += weaponStateMachine.OnChangeWeapon;
        WeaponControllerManager.OnGlobalFire += OnGlobalFireReceived;
        WeaponControllerManager.OnGlobalFireReleased += OnGlobalFireReleasedReceived;
        WeaponControllerManager.OnReload += OnReloadReceived;
    }

    private void OnDestroy()
    {
        weapon.OnWeaponEquipped -= weaponStateMachine.OnChangeWeapon;
        WeaponControllerManager.OnGlobalFire -= OnGlobalFireReceived;
        WeaponControllerManager.OnGlobalFireReleased -= OnGlobalFireReleasedReceived;
        WeaponControllerManager.OnReload -= OnReloadReceived;
    }

    public void Update()
    {
        weaponStateMachine.Update();
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
        if (weapon.WeaponData == null || !IsInputEnabled)
        {
            return;
        }

        if (context.phase == InputActionPhase.Started)
        {
            isFirePressed = true;
            weaponStateMachine.OnFire();
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
        if (weapon.WeaponData == null || !IsInputEnabled)
        {
            return;
        }

        if (context.phase == InputActionPhase.Performed)
        {
            weaponStateMachine.OnReload();
            WeaponControllerManager.BroadcastReload();
        }
    }

    private void OnGlobalFireReceived()
    {
        if (isFirePressed) return;
        if (weapon.WeaponData == null || !IsInputEnabled) return;

        isFirePressed = true;
        weaponStateMachine.OnFire();
    }

    private void OnGlobalFireReleasedReceived()
    {
        if (!isFirePressed) return;
        isFirePressed = false;
    }

    private void OnReloadReceived()
    {
        if (weapon.WeaponData == null || !IsInputEnabled) return;

        weaponStateMachine.OnReload();
    }
}