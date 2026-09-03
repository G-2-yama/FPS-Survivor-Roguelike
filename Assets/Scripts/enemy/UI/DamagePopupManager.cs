using UnityEngine;

public class DamageEffectManager : MonoBehaviour
{
    public static DamageEffectManager Instance { get; private set; }

    [SerializeField] private GameObject damagePopupPrefab;
    [SerializeField] private GameObject damageEffectPrefab;

    [SerializeField] private Transform player;

    [SerializeField] private float heightOffset = 1.5f;

    [SerializeField] private float randomOffsetX = 0.3f;
    [SerializeField] private float randomOffsetZ = 0.3f;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDamage(int damage, Vector3 enemyPosition)
    {
        Vector3 position = enemyPosition + Vector3.up * heightOffset;

        // ダメージポップアップ
        position.x += Random.Range(-randomOffsetX, randomOffsetX);

        position.z += Random.Range(-randomOffsetZ, randomOffsetZ);

        GameObject popupObj = PoolManager.Instance.Get(damagePopupPrefab);

        DamagePopup popup = popupObj.GetComponent<DamagePopup>();

        popup.Setup(damage,position, player);

        // ダメージエフェクト
        GameObject effectObj = PoolManager.Instance.Get(damageEffectPrefab);

        effectObj.transform.position = enemyPosition;
        effectObj.transform.rotation = Quaternion.identity;
    }
}