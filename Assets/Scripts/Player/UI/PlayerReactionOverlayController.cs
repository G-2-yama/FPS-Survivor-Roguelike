using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤーの反応イベントを購読し、対応するオーバーレイ状態を更新するコントローラー。
/// </summary>
[DisallowMultipleComponent]
public class PlayerReactionOverlayController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Player player;

    [Header("Overlay")]
    [SerializeField] private RectTransform overlayRoot;
    [SerializeField] private Image blackOverlayImage;
    [SerializeField] private ScreenVignetteGraphic edgeOverlayGraphic;

    [Header("State Settings")]
    [SerializeField] private DamagedOverlayStateSettings damagedOverlaySettings;
    [SerializeField] private HealedOverlayStateSettings healedOverlaySettings;

    [Header("Vignette Shape")]
    [SerializeField, Range(0f, 1f)] private float vignetteInnerRadius = 0.55f;
    [SerializeField, Range(0f, 1.5f)] private float vignetteOuterRadius = 1f;

    private OverlayStateManager stateManager;
    private PlayerReactionOverlayView overlayView;
    private bool isSubscribed;

    private void Awake()
    {
        ResolvePlayerIfNeeded();
        CacheOverlayReferences();
        ValidateConfiguration();

        overlayView = new PlayerReactionOverlayView(
            blackOverlayImage,
            edgeOverlayGraphic,
            vignetteInnerRadius,
            vignetteOuterRadius);
        overlayView.Reset();

        stateManager = new OverlayStateManager(new IOverlayState[]
        {
            new DamagedOverlayState(damagedOverlaySettings),
            new HealedOverlayState(healedOverlaySettings),
        });
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        stateManager?.Reset();
        overlayView?.Reset();
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

        if (stateManager == null || overlayView == null)
        {
            return;
        }

        OverlayEffectFrame frame = stateManager.Tick(Time.unscaledDeltaTime);
        overlayView.Render(frame);
    }

    private void HandleDamaged(int damage, int currentHp, int maxHp)
    {
        stateManager?.Trigger(PlayerOverlayTrigger.Damaged, damage, maxHp);
    }

    private void HandleHealed(int amount, int currentHp, int maxHp)
    {
        stateManager?.Trigger(PlayerOverlayTrigger.Healed, amount, maxHp);
    }

    private void TrySubscribe()
    {
        if (player?.Health == null)
        {
            return;
        }

        player.Health.OnDamaged -= HandleDamaged;
        player.Health.OnDamaged += HandleDamaged;
        player.Health.OnHealed -= HandleHealed;
        player.Health.OnHealed += HandleHealed;
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
        player.Health.OnHealed -= HandleHealed;
        isSubscribed = false;
    }

    private void ResolvePlayerIfNeeded()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
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

        if (overlayRoot != null && edgeOverlayGraphic == null)
        {
            edgeOverlayGraphic = overlayRoot.GetComponentInChildren<ScreenVignetteGraphic>(true);
        }
    }

    private void ValidateConfiguration()
    {
        if (overlayRoot == null)
        {
            Debug.LogWarning($"{nameof(PlayerReactionOverlayController)} requires {nameof(overlayRoot)}.", this);
        }

        if (blackOverlayImage == null)
        {
            Debug.LogWarning($"{nameof(PlayerReactionOverlayController)} requires {nameof(blackOverlayImage)}.", this);
        }

        if (edgeOverlayGraphic == null)
        {
            Debug.LogWarning($"{nameof(PlayerReactionOverlayController)} requires {nameof(edgeOverlayGraphic)}.", this);
        }

        if (damagedOverlaySettings == null)
        {
            Debug.LogWarning($"{nameof(PlayerReactionOverlayController)} has no {nameof(damagedOverlaySettings)} assigned. Using built-in defaults.", this);
        }

        if (healedOverlaySettings == null)
        {
            Debug.LogWarning($"{nameof(PlayerReactionOverlayController)} has no {nameof(healedOverlaySettings)} assigned. Using built-in defaults.", this);
        }
    }
}
