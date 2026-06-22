using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/StaticAreaFire")]
public class StaticAreaFire : FireModeData
{
    [SerializeField] private GameObject damageAreaPrefab;

    [SerializeField] private Vector3 distance = new Vector3(0f, 0f, 0f);

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Transform cameraTransform = Camera.main.transform;

        Vector3 offset = cameraTransform.TransformDirection(distance);

        Vector3 spawnPos = cameraTransform.position + offset;

        GameObject area = PoolManager.Instance.Get(damageAreaPrefab);

        area.transform.position = spawnPos;

        area.transform.rotation = cameraTransform.rotation;
        area.transform.rotation = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f);

        area.GetComponent<DamageArea>().Initialize(GetDamageAmount(weapon, weaponOwner), GetKnockbackForce(weapon, weaponOwner));
    }
}