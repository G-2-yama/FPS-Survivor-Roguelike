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
        if (!weapon.HasWeapon || !IsInputEnabled)
        {
            return;
        }

        if (context.phase == InputActionPhase.Started)
        {
            isFirePressed = true;
            weaponStateMachine.OnFire();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            isFirePressed = false;
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
            weaponStateMachine.OnReload();
        }
    }
}