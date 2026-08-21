using UnityEngine;
using System.Collections;

public class BeamSwordEffect : MonoBehaviour
{
    private Vector3 defaultScale;

    [SerializeField]
    [Range(0f, 1f)]
    private float scaleChangeRate = 0.9f;

    [SerializeField]
    private float scaleChangeTime = 0.1f;

    private void Start()
    {
        defaultScale = transform.localScale;

        StartCoroutine(EffectCoroutine());
    }

    private IEnumerator EffectCoroutine()
    {
        Vector3 smallScale = new Vector3(
            defaultScale.x,
            defaultScale.y * scaleChangeRate,
            defaultScale.z * scaleChangeRate
        );

        while (true)
        {
            // å≥ÇÃëÂÇ´Ç≥
            transform.localScale = defaultScale;

            yield return new WaitForSeconds(scaleChangeTime);

            // è≠Çµè¨Ç≥Ç¢ëÂÇ´Ç≥
            transform.localScale = smallScale;

            yield return new WaitForSeconds(scaleChangeTime);
        }
    }
}