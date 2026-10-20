using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingRowView : SettingsMenuItem, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("設定")]
    [Tooltip("この行で変える設定")]
    [SerializeField] private GameSettingType setting;

    [Header("表示")]
    [Tooltip("値を目盛りで表すバー 押す・ドラッグすると値が変わります")]
    [SerializeField] private SettingBarGraphic bar;

    [Tooltip("値の数字")]
    [SerializeField] private TMP_Text valueText;

    [Tooltip("値を1つ下げるボタン")]
    [SerializeField] private Button decreaseButton;

    [Tooltip("値を1つ上げるボタン")]
    [SerializeField] private Button increaseButton;

    [Tooltip("下げるボタンの三角")]
    [SerializeField] private Graphic decreaseArrow;

    [Tooltip("上げるボタンの三角")]
    [SerializeField] private Graphic increaseArrow;

    [Header("色と大きさ")]
    [Tooltip("値の数字の色")]
    [SerializeField] private Color valueColor = new Color(0.36f, 0.78f, 1f);

    [Tooltip("値が0のときに出す OFF の色")]
    [SerializeField] private Color offColor = new Color(0.54f, 0.56f, 0.62f);

    [Tooltip("値の数字の大きさ")]
    [SerializeField] private float valueFontSize = 46f;

    [Tooltip("OFF の大きさ")]
    [SerializeField] private float offFontSize = 26f;

    [Tooltip("行を選んでいないときの三角の色")]
    [SerializeField] private Color arrowColor = new Color(0.54f, 0.56f, 0.62f);

    [Tooltip("行を選んでいるときの三角の色")]
    [SerializeField] private Color arrowFocusedColor = new Color(0.96f, 0.96f, 0.97f);

    [Tooltip("値を変えた瞬間の三角の色")]
    [SerializeField] private Color arrowHitColor = new Color(0.36f, 0.78f, 1f);

    [Tooltip("値を変えた瞬間の三角の拡大率")]
    [SerializeField] private float arrowHitScale = 1.3f;

    [Tooltip("値を変えた瞬間の見た目を保つ時間（秒）")]
    [SerializeField] private float arrowHitDuration = 0.12f;

    private bool isFocused;
    private bool isDragging;
    private float decreaseHitAt = float.NegativeInfinity;
    private float increaseHitAt = float.NegativeInfinity;

    private void Awake()
    {
        decreaseButton.onClick.AddListener(() => Step(-1));
        increaseButton.onClick.AddListener(() => Step(1));
    }

    private void OnEnable()
    {
        GameSettings.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        GameSettings.Changed -= Refresh;
        isDragging = false;
        decreaseHitAt = increaseHitAt = float.NegativeInfinity;
        UpdateArrows();
    }

    private void Update()
    {
        UpdateArrows();
    }

    public override void SetFocused(bool focused)
    {
        base.SetFocused(focused);
        isFocused = focused;
        bar.Highlighted = focused;
    }

    public override void Step(int direction)
    {
        if (direction < 0)
            decreaseHitAt = Time.unscaledTime;
        else if (direction > 0)
            increaseHitAt = Time.unscaledTime;

        GameSettings.SetLevel(setting, GameSettings.GetLevel(setting) + direction);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerPressRaycast.gameObject != bar.gameObject)
            return;

        isDragging = true;
        SetFromPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
            SetFromPointer(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void SetFromPointer(PointerEventData eventData)
    {
        var rectTransform = bar.rectTransform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var local))
            return;

        Rect rect = rectTransform.rect;
        float ratio = Mathf.InverseLerp(rect.xMin, rect.xMax, local.x);
        GameSettings.SetLevel(setting, Mathf.CeilToInt(ratio * bar.Steps));
    }

    private void Refresh()
    {
        int level = GameSettings.GetLevel(setting);
        bar.Level = level;

        bool isOff = level == 0;
        valueText.text = isOff ? "OFF" : level.ToString();
        valueText.color = isOff ? offColor : valueColor;
        valueText.fontSize = isOff ? offFontSize : valueFontSize;
    }

    private void UpdateArrows()
    {
        ApplyArrow(decreaseArrow, decreaseHitAt);
        ApplyArrow(increaseArrow, increaseHitAt);
    }

    private void ApplyArrow(Graphic arrow, float hitAt)
    {
        bool isHit = Time.unscaledTime - hitAt < arrowHitDuration;
        arrow.color = isHit ? arrowHitColor : isFocused ? arrowFocusedColor : arrowColor;
        arrow.rectTransform.localScale = Vector3.one * (isHit ? arrowHitScale : 1f);
    }
}
