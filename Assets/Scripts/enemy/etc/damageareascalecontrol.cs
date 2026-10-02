using UnityEngine;
using System.Collections.Generic;

public class DamageAreaScaleController : MonoBehaviour
{
    [System.Serializable]
    private class ScaleData
    {
        [Min(0)]
        public int damageThreshold;

        public Vector3 scale = Vector3.one;
    }

    [SerializeField]
    private List<ScaleData> scaleDatas = new();

    [SerializeField]
    private Vector3 defaultScale = Vector3.one;
    [SerializeField] private Transform effectTransform;

    public void SetScaleByDamage(int damage)
    {
        Vector3 selectedScale = defaultScale;
        int highestThreshold = int.MinValue;

        foreach (var data in scaleDatas)
        {
            if (damage >= data.damageThreshold &&
                data.damageThreshold > highestThreshold)
            {
                selectedScale = data.scale;
                highestThreshold = data.damageThreshold;
            }
        }

        transform.localScale = selectedScale;
        effectTransform.localScale = selectedScale;
    }

    public void ResetScale()
    {
        transform.localScale = defaultScale;
    }
}