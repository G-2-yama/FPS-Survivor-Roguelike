using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;

    public WeaponData WeaponData => weaponData;

    private WeaponStateMachine weaponStateMachine;
    public WeaponStateMachine WeaponStateMachine => weaponStateMachine;

    /// <summary>
    /// 初期化
    /// </summary>
    void Awake()
    {
        weaponStateMachine = new WeaponStateMachine();
    }

    /// <summary>
    /// 初期状態として待機状態に遷移
    /// </summary>
    void Start()
    {
        weaponStateMachine.ChangeState(new WeaponIdleState(this));
    }

    public void Update()
    {
        weaponStateMachine.Update();
    }

    /// <summary>
    /// 攻撃入力を処理するメソッド
    /// </summary>
    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            weaponStateMachine.OnFire();
        }
    }

    /// <summary>
    /// リロード入力を処理するメソッド
    /// </summary>
    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            weaponStateMachine.OnReload();
        }
    }
}