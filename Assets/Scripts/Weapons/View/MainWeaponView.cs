using UnityEngine;
using UnityEngine.UI;

public class MainWeaponView : WeaponView
{
    [SerializeField] private Text currentAmmoText;
    [SerializeField] private Text magazineSizeText;
    [SerializeField] private float idleMoveAmount = 0.03f;
    [SerializeField] private float idleMoveSpeed = 2f;
    [SerializeField] private float idleRotationAmount = 2f;

    // 武器の通常時のモーションの初期位置と回転を保持するための変数
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    protected override void Start()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;

        base.Start();
    }

    protected virtual void Update()
    {
        UpdateIdleMotion();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void RefreshView(Weapon weapon)
    {
        base.RefreshView(weapon);

        SetAmmoUIVisible(weapon != null && weapon.HasWeapon);

        if (weapon == null || !weapon.HasWeapon)
            return;

        UpdateAmmo(
            weapon.CurrentAmmo,
            weapon.WeaponData.MagazineSize
        );
    }

    public override void PlayReloadAnimation()
    {
        animator?.SetTrigger("Reload");
    }

    public override void PlayFireAnimation()
    {
        animator?.SetTrigger("Fire");
    }

    /// <summary>
    /// 武器の通常時のモーションを更新する
    /// </summary>
    private void UpdateIdleMotion()
    {
        if (weaponModelInstance == null)
            return;

        float time = Time.time * idleMoveSpeed;

        // 上下の移動と左右の揺れ
        float vertical = Mathf.Sin(time) * idleMoveAmount;
        float horizontal = Mathf.Sin(time * 0.5f) * idleMoveAmount * 0.5f;
        transform.localPosition = initialLocalPosition + new Vector3(horizontal, vertical, 0f);

        // 回転の揺れ
        float rotation = Mathf.Sin(time * 0.5f) * idleRotationAmount;
        transform.localRotation = initialLocalRotation * Quaternion.Euler(0f, 0f, rotation);
    }

    private void SetAmmoUIVisible(bool visible)
    {
        currentAmmoText.gameObject.SetActive(visible);
        magazineSizeText.gameObject.SetActive(visible);
    }

    private void UpdateAmmo(int current, int max)
    {
        currentAmmoText.text = current.ToString();
        magazineSizeText.text = "/" + max;
    }
}