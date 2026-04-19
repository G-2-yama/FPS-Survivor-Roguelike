using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/HitScan")]
public class HitScanData : FireModeData
{
    [SerializeField] private float maxRange = 60f;
    public float MaxRange => maxRange;

    /// <inheritdoc />
    public override void Fire(Weapon weapon)
    {
        Vector3 direction = GetFireDirection(weapon);
        Ray ray = new Ray(Camera.main.transform.position, direction);

        Debug.DrawRay(ray.origin, ray.direction * maxRange, Color.red, 1f);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
        {
            TryApplyDamage(weapon, hit.collider);
        }
    }
}
