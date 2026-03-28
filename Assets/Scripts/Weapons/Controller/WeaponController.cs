using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;

    public WeaponData Model => weaponData;

    private void Start()
    {

    }

    /// <summary>
    /// 攻撃入力を処理するメソッド
    /// </summary>
    public void OnFire(InputAction.CallbackContext context)
    {
        // ここで攻撃処理を実装
        Debug.Log($"Firing weapon: {weaponData.DisplayName}");
    }

    /// <summary>
    /// リロード入力を処理するメソッド
    /// </summary>
    public void OnReload(InputAction.CallbackContext context)
    {
        // ここでリロード処理を実装
        Debug.Log($"Reloading weapon: {weaponData.DisplayName}");
    }
}