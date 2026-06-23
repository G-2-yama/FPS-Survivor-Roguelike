using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤー被ダメージ時に画面を暗くし、外周に赤いビネットを表示する。
/// </summary>
[DisallowMultipleComponent]
public class PlayerDamageOverlayView : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Player player;

    [Header("Overlay")]
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private Image blackOverlayImage;
    [SerializeField] private ScreenVignetteGraphic redVignetteGraphic;

    [Header("Intensity")]
    [SerializeField, Range(0f, 1f)] private float minDamageIntensity = 0.35f;
    [SerializeField, Range(0f, 2f)] private float damageToIntensityScale = 4f;
    [SerializeField, Range(0f, 1f)] private float stackedIntensityGain = 0.25f;
    [SerializeField, Range(0f, 1f)] private float maxBlackAlpha = 0.18f;
    [SerializeField, Range(0f, 1f)] private float maxRedAlpha = 0.5f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float fadeInDuration = 0.06f;
    [SerializeField, Min(0f)] private float holdDuration = 0.08f;
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 0.35f;

    [Header("Vignette Shape")]
    [SerializeField, Range(0f, 1f)] private float vignetteInnerRadius = 0.55f;
    [SerializeField, Range(0f, 1.5f)] private float vignetteOuterRadius = 1f;

    private Coroutine animationCoroutine;
    private float currentIntensity;
    private bool isSubscribed;

    private void Awake()
    {
        ResolvePlayerIfNeeded();
        CacheOverlayReferences();
        ValidateConfiguration();
        ApplyShapeSettings();
        ApplyVisuals(0f);
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        ValidateConfiguration();
        ApplyVisuals(0f);
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (!isSubscribed)
        {
            TrySubscribe();
        }
    }

    private void HandleDamaged(int damage, int currentHp, int maxHp)
    {
        float normalizedDamage = maxHp > 0 ? (float)damage / maxHp : 0f;
        float nextIntensity = Mathf.Clamp01(
            Mathf.Max(minDamageIntensity, normalizedDamage * damageToIntensityScale)
            + currentIntensity * stackedIntensityGain);

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(PlayDamageOverlay(nextIntensity));
    }

    private IEnumerator PlayDamageOverlay(float targetIntensity)
    {
        float startIntensity = currentIntensity;

        if (fadeInDuration <= 0f)
        {
            currentIntensity = targetIntensity;
            ApplyVisuals(currentIntensity);
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / fadeInDuration);
                ApplyVisuals(currentIntensity);
                yield return null;
            }
        }

        currentIntensity = targetIntensity;
        ApplyVisuals(currentIntensity);

        if (holdDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(holdDuration);
        }

        float fadeElapsed = 0f;
        float fadeStartIntensity = currentIntensity;
        while (fadeElapsed < fadeOutDuration)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            currentIntensity = Mathf.Lerp(fadeStartIntensity, 0f, fadeElapsed / fadeOutDuration);
            ApplyVisuals(currentIntensity);
            yield return null;
        }

        currentIntensity = 0f;
        ApplyVisuals(0f);
        animationCoroutine = null;
    }

    private void TrySubscribe()
    {
        if (player?.Health == null)
        {
            return;
        }

        player.Health.OnDamaged -= HandleDamaged;
        player.Health.OnDamaged += HandleDamaged;
        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (player?.Health == null)
        {
            isSubscribed = false;
            return;
        }

        player.Health.OnDamaged -= HandleDamaged;
        isSubscribed = false;
    }

    private void ResolvePlayerIfNeeded()
    {
        if (player != null)
        {
            return;
        }

        player = FindObjectOfType<Player>();
    }

    private void CacheOverlayReferences()
    {
        if (overlayRoot == null)
        {
            overlayRoot = transform as RectTransform;
        }

        if (overlayRoot != null && blackOverlayImage == null)
        {
            blackOverlayImage = overlayRoot.GetComponentInChildren<Image>(true);
        }

        if (overlayRoot != null && redVignetteGraphic == null)
        {
            redVignetteGraphic = overlayRoot.GetComponentInChildren<ScreenVignetteGraphic>(true);
        }
    }

    private void ValidateConfiguration()
    {
        if (overlayRoot == null)
        {
            Debug.LogWarning($"{nameof(PlayerDamageOverlayView)} requires {nameof(overlayRoot)}.", this);
        }

        if (blackOverlayImage == null)
        {
            Debug.LogWarning($"{nameof(PlayerDamageOverlayView)} requires {nameof(blackOverlayImage)}.", this);
        }

        if (redVignetteGraphic == null)
        {
            Debug.LogWarning($"{nameof(PlayerDamageOverlayView)} requires {nameof(redVignetteGraphic)}.", this);
        }

        if (blackOverlayImage != null)
        {
            blackOverlayImage.raycastTarget = false;
        }

        if (redVignetteGraphic != null)
        {
            redVignetteGraphic.raycastTarget = false;
        }
    }

    private void ApplyShapeSettings()
    {
        if (redVignetteGraphic == null)
        {
            return;
        }

        redVignetteGraphic.InnerRadius = vignetteInnerRadius;
        redVignetteGraphic.OuterRadius = vignetteOuterRadius;
    }

    private void ApplyVisuals(float intensity)
    {
        if (blackOverlayImage != null)
        {
            blackOverlayImage.color = new Color(0f, 0f, 0f, maxBlackAlpha * intensity);
        }

        if (redVignetteGraphic != null)
        {
            redVignetteGraphic.color = new Color(0.85f, 0.1f, 0.08f, maxRedAlpha * intensity);
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!gameObject.scene.IsValid())
        {
            return;
        }

        CacheOverlayReferences();
        ValidateConfiguration();
        ApplyShapeSettings();
        ApplyVisuals(currentIntensity);
    }
#endif
}
