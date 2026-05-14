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
        weapon.OnWeaponEquipped += HandleWeaponEquipped;

        RefreshView();
    }

    protected virtual void OnDestroy()
    {
        if (weapon != null)
        {
            weapon.OnWeaponEquipped -= HandleWeaponEquipped;
        }

        ClearWeaponModel();
    }

    protected virtual void HandleWeaponEquipped(WeaponData data)
    {
        RefreshView();
    }

    protected virtual void RefreshView()
    {
        var data = weapon.WeaponData;

        bool hasWeapon = data != null;

        weaponIcon.gameObject.SetActive(hasWeapon);

        if (!hasWeapon)
        {
            ClearWeaponModel();
            return;
        }

        weaponIcon.sprite = data.Icon;

        SetWeaponModel(data);
    }

    protected void SetWeaponModel(WeaponData data)
    {
        ClearWeaponModel();

        if (data.WeaponModelPrefab == null)
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

    public virtual void SetReloadProgress(float progress)
    { 
        reloadIndicator.fillAmount = progress;
    }

    public virtual void PlayReloadAnimation() { }

    public virtual void PlayFireAnimation() { }
}
