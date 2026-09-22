using UnityEngine;
using UnityEngine.UI;

public abstract class WeaponView : MonoBehaviour
{
    [SerializeField] protected Weapon weapon;
    [SerializeField] protected Image weaponIcon;
    [SerializeField] private Image reloadIndicator;

    protected GameObject weaponModelInstance;
    protected Animator animator;
    protected WeaponAnimationTiming animationTiming;
    private GameObject currentModelPrefab;

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

    public virtual void UpdateAmmo(int current, int max) { }

    public virtual void SetReloadProgress(float progress)
    {
        reloadIndicator.fillAmount = progress;
    }

    public virtual void PlayReloadAnimation(float duration) { }

    public virtual void PlayFireAnimation(float duration) { }


    protected void SetWeaponModel(WeaponData data)
    {
        GameObject prefab = data != null ? data.WeaponModelPrefab : null;
        if (weaponModelInstance != null && currentModelPrefab == prefab)
            return;

        ClearWeaponModel();

        if (prefab == null)
            return;

        weaponModelInstance = Instantiate(prefab, transform);
        currentModelPrefab = prefab;
        animator = weaponModelInstance.GetComponent<Animator>();
        animationTiming = weaponModelInstance.GetComponent<WeaponAnimationTiming>();
    }

    protected void ClearWeaponModel()
    {
        if (weaponModelInstance != null)
        {
            Destroy(weaponModelInstance);
            weaponModelInstance = null;
        }

        animator = null;
        animationTiming = null;
        currentModelPrefab = null;
    }

    private void SetWeaponInactive()
    {
        ClearWeaponModel();
    }
}
