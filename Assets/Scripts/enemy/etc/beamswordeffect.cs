using UnityEngine;
using System.Collections;

public class BeamSwordEffect : EnemyAnimationEffect
{
    [SerializeField] private GameObject beamSword;
    [SerializeField] private Transform beamSwordTransform;

    private Vector3 defaultScale;

    [SerializeField]
    [Range(0f, 1f)]
    private float scaleChangeRate = 0.9f;

    [SerializeField]
    private float scaleChangeTime = 0.1f;

    private Coroutine effectCoroutine;

    private void Awake()
    {
       
        defaultScale = beamSwordTransform.localScale;

        beamSwordTransform.localScale = defaultScale;
        beamSword.SetActive(false);
    }

    public override void Play()
    {
        // すでに動いていたら止める
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }

        // 必ず本来のサイズに戻してから表示
        beamSwordTransform.localScale = defaultScale;

        beamSword.SetActive(true);

        effectCoroutine = StartCoroutine(EffectCoroutine());
    }

    public override void Stop()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }

        // Poolへ戻る前に必ず初期Scaleへ
        beamSwordTransform.localScale = defaultScale;

        beamSword.SetActive(false);
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
            beamSwordTransform.localScale = defaultScale;

            yield return new WaitForSeconds(scaleChangeTime);

            beamSwordTransform.localScale = smallScale;

            yield return new WaitForSeconds(scaleChangeTime);
        }
    }
}