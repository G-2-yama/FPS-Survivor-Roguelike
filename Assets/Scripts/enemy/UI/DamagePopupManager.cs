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
    [SerializeField] private Color defaultDamageColor;

    [System.Serializable]
    private class ChangeDamagePopupMaterial
    {
        public Color damageColor = Color.white;
        public int damageRange;
    }

    [SerializeField]
    private ChangeDamagePopupMaterial[] changeDamagePopupMaterials;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowDamage(int damage, Vector3 enemyPosition)
    {
        Vector3 position =
            enemyPosition + Vector3.up * heightOffset;

        position.x += Random.Range(-randomOffsetX, randomOffsetX);
        position.z += Random.Range(-randomOffsetZ, randomOffsetZ);

        // -----------------------------
        // ダメージ量から色を決定
        // -----------------------------
        Color popupColor = GetDamageColor(damage);

        // -----------------------------
        // ダメージポップアップ
        // -----------------------------
        GameObject popupObj =
            PoolManager.Instance.Get(damagePopupPrefab);

        DamagePopup popup =
            popupObj.GetComponent<DamagePopup>();

        popup.Setup(
            popupColor,
            damage,
            position,
            player
        );

        // -----------------------------
        // ダメージエフェクト
        // -----------------------------
        GameObject effectObj =
            PoolManager.Instance.Get(damageEffectPrefab);

        effectObj.transform.position = enemyPosition;
        effectObj.transform.rotation = Quaternion.identity;
    }

    private Color GetDamageColor(int damage)
    {
        Color resultColor = defaultDamageColor;
        int highestThreshold = int.MinValue;

        foreach (var setting in changeDamagePopupMaterials)
        {
            // damage以下の閾値で、今まで見つけたものより大きい閾値を採用
            if (damage >= setting.damageRange &&
                setting.damageRange > highestThreshold)
            {
                highestThreshold = setting.damageRange;
                resultColor = setting.damageColor;
            }
        }

        return resultColor;
    }
}