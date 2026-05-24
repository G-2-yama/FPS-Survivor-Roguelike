using UnityEngine;
using UnityEngine.UI;

public abstract class WeaponView : MonoBehaviour
{
    [SerializeField] protected Weapon weapon;
    [SerializeField] protected Image weaponIcon;
    [SerializeField] private Image reloadIndicator;

    protected GameObject weaponModelInstance;
    protected Animator animator;

    protected virtual void Start()
    {
        RefreshView(weapon);
    }

    protected virtual void OnDestroy()
    {
        ClearWeaponModel();
    }

    public virtual void RefreshView(Weapon weapon)
    {
        if (weapon == null)
        {
            SetWeaponInactive();
            return;
        }

        weaponIcon.gameObject.SetActive(weapon.HasWeapon);

        if (!weapon.HasWeapon)
        {
            SetWeaponInactive();
            return;
        }

        weaponIcon.sprite = weapon.WeaponData.Icon;

        SetWeaponModel(weapon.WeaponData);
    }

    public virtual void SetReloadProgress(float progress)
    {
        reloadIndicator.fillAmount = progress;
    }

    public virtual void PlayReloadAnimation() { }

    public virtual void PlayFireAnimation() { }


    protected void SetWeaponModel(WeaponData data)
    {
        ClearWeaponModel();

        if (data == null || data.WeaponModelPrefab == null)
            return;

        weaponModelInstance = Instantiate(data.WeaponModelPrefab, transform);
        animator = weaponModelInstance.GetComponent<Animator>();
    }

    protected void ClearWeaponModel()
    {
        if (weaponModelInstance != null)
        {
            Destroy(weaponModelInstance);
            weaponModelInstance = null;
        }

        animator = null;
    }

    private void SetWeaponInactive()
    {
        ClearWeaponModel();
    }
}