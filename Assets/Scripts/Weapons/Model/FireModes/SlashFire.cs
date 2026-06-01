using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/FireMode/SlashFire")]
public class SlashFire : FireModeData
{
    [SerializeField] private GameObject damageAreaPrefab;

    [SerializeField] private float distance = 2f;

    public override void Fire(Weapon weapon, Player weaponOwner)
    {
        Vector3 direction = Camera.main.transform.forward;

        Vector3 spawnPos = Camera.main.transform.position + direction * distance;

        GameObject area = PoolManager.Instance.Get(damageAreaPrefab);

        area.transform.position = spawnPos;
        area.transform.rotation = Quaternion.LookRotation(direction);

        area.GetComponent<DamageArea>().Initialize(GetDamageAmount(weapon, weaponOwner));
    }
}