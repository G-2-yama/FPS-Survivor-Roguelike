using UnityEngine;
using System.Collections;

public class BeamSwordEffect : EnemyAnimationEffect
{
    [SerializeField]private GameObject beamSword;
    [SerializeField]private Transform beamSwordTransform;
    private Vector3 defaultScale;

    [SerializeField]
    [Range(0f, 1f)]
    private float scaleChangeRate = 0.9f;

    [SerializeField]
    private float scaleChangeTime = 0.1f;
    private void Awake()
    {
        beamSword.SetActive(false);
        defaultScale = beamSwordTransform.localScale;
    }

    public override void Play()
    {
        
        beamSword.SetActive(true);
        defaultScale = beamSwordTransform.localScale;

        StartCoroutine(EffectCoroutine());
    }
    public override void Stop()
    {
        beamSword.SetActive(false);
        StopAllCoroutines();
        beamSwordTransform.localScale = defaultScale;
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
            beamSwordTransform.localScale = defaultScale;

            yield return new WaitForSeconds(scaleChangeTime);

            // è≠Çµè¨Ç≥Ç¢ëÂÇ´Ç≥
            beamSwordTransform.localScale = smallScale;

            yield return new WaitForSeconds(scaleChangeTime);
        }
    }
}