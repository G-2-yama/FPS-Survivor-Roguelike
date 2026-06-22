using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/TrackingAreaFire")]
public class TrackingAreaFire : FireModeData
{
    [SerializeField] private GameObject damageAreaPrefab;

    [SerializeField] private float distance = 0.1f;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = Camera.main.transform.forward;

        Vector3 spawnPos = Camera.main.transform.position - Camera.main.transform.up * distance;

        GameObject area = PoolManager.Instance.Get(damageAreaPrefab);


        area.transform.position = spawnPos;
        area.transform.rotation = Quaternion.LookRotation(direction);
        area.transform.SetParent(Camera.main.transform);

        area.GetComponent<DamageArea>().Initialize(GetDamageAmount(weapon, weaponOwner), GetKnockbackForce(weapon, weaponOwner));
    }
}