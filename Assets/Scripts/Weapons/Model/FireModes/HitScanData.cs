using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/HitScan")]
public class HitScanData : FireModeData
{
    [SerializeField] private float maxRange = 60f;
    [SerializeField] private float hitRadius = 0.1f;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = GetFireDirection(weapon);
        Ray ray = new Ray(Camera.main.transform.position, direction);

        Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 1f);

        if (Physics.SphereCast(ray, hitRadius, out RaycastHit hit, maxRange))
        {
            TryApplyDamage(weapon, hit.collider, weaponOwner);
            TryEnableHitEffect(out GameObject hitEffect);
            if (hitEffect != null)
            {
                hitEffect.transform.position = hit.point;
            }
        }
    }
}