using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [SerializeField] private GameObject damagePopupPrefab;

    [SerializeField] private Transform player;

    [SerializeField] private float heightOffset = 1.5f;

    [SerializeField] private float randomOffsetX = 0.3f;
    [SerializeField] private float randomOffsetZ = 0.3f;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDamage(
        int damage,
        Vector3 enemyPosition)
    {
        GameObject obj =
            PoolManager.Instance.Get(damagePopupPrefab);

        Vector3 position =
            enemyPosition + Vector3.up * heightOffset;

        // è≠ÇµéUÇÁÇ∑
        position.x += Random.Range(
            -randomOffsetX,
            randomOffsetX);

        position.z += Random.Range(
            -randomOffsetZ,
            randomOffsetZ);

        DamagePopup popup =
            obj.GetComponent<DamagePopup>();

        popup.Setup(
            damage,
            position,
            player
        );
    }
}