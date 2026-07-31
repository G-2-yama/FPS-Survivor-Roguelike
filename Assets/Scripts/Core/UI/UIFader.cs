using System;
using System.Collections;
using UnityEngine;

public class UIFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject brockImage;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine fadeCoroutine;

    public void FadeIn()
    {
        brockImage?.SetActive(true);
        StartFade(1f, null);
    }

    public void FadeOut(Action onComplete = null)
    {
        StartFade(0f, onComplete);
    }

    private void StartFade(float targetAlpha, Action onComplete)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine =
            StartCoroutine(Fade(targetAlpha, onComplete));
    }

    private IEnumerator Fade(float targetAlpha, Action onComplete)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;

            canvasGroup.alpha =
                Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        onComplete?.Invoke();
        brockImage?.SetActive(false);
    }
}