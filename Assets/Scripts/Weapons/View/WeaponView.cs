using UnityEngine;
using UnityEngine.UI;

public class WeaponView : MonoBehaviour
{
    [SerializeField] private Weapon weapon;

    [SerializeField] private Text currentAmmoText;


    private GameObject weaponModelInstance;

    /// <summary>
    /// 初期化時にイベント購読を行い、初期表示状態を設定する
    /// </summary>
    private void Start()
    {
        weapon.OnAmmoChanged += UpdateAmmo;
        weapon.OnWeaponEquipped += HandleWeaponEquipped;

        if (weapon.WeaponData == null)
        {
            currentAmmoText.gameObject.SetActive(false);
            ClearWeaponModel();
            return;
        }

        SetWeaponModel(weapon.WeaponData);
        currentAmmoText.gameObject.SetActive(true);
        UpdateAmmo(weapon.CurrentAmmo, weapon.WeaponStats.MagazineSize);
    }

    /// <summary>
    /// 破棄時にイベント購読を解除し、表示中モデルを後始末する
    /// </summary>
    private void OnDestroy()
    {
        if (weapon != null)
        {
            weapon.OnAmmoChanged -= UpdateAmmo;
            weapon.OnWeaponEquipped -= HandleWeaponEquipped;
        }

        ClearWeaponModel();
    }

    /// <summary>
    /// 武器装備イベントを受け取り、表示モデルを切り替える
    /// </summary>
    /// <param name="data">装備された武器データ</param>
    private void HandleWeaponEquipped(WeaponData data)
    {
        SetWeaponModel(data);
    }

    /// <summary>
    /// 残弾表示を更新する
    /// </summary>
    /// <param name="current">現在弾数</param>
    /// <param name="max">最大弾数</param>
    private void UpdateAmmo(int current, int max)
    {


        currentAmmoText.text = $"{current} / {max}";
    }

    /// <summary>
    /// 指定された武器データに応じて武器モデルを生成して表示する
    /// </summary>
    /// <param name="data">表示対象の武器データ</param>
    private void SetWeaponModel(WeaponData data)
    {
        ClearWeaponModel();
        currentAmmoText.gameObject.SetActive(true);
        weaponModelInstance = Instantiate(data.WeaponModelPrefab, transform);
    }

    /// <summary>
    /// 現在表示中の武器モデルを破棄する
    /// </summary>
    private void ClearWeaponModel()
    {
        Destroy(weaponModelInstance);
        weaponModelInstance = null;
    }

}