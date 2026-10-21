using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialToggleRow : SettingsMenuItem
{
    [Header("値")]
    [Tooltip("最初の値")]
    [SerializeField] private bool value = true;

    [Header("表示")]
    [Tooltip("ON / OFF を出すテキスト")]
    [SerializeField] private TMP_Text valueText;

    [Tooltip("ON のときに出す文字")]
    [SerializeField] private string onText = "ON";

    [Tooltip("OFF のときに出す文字")]
    [SerializeField] private string offText = "OFF";

    [Tooltip("値を切り替えるボタン（左）")]
    [SerializeField] private Button decreaseButton;

    [Tooltip("値を切り替えるボタン（右）")]
    [SerializeField] private Button increaseButton;

    [Tooltip("左のボタンの三角")]
    [SerializeField] private Graphic decreaseArrow;

    [Tooltip("右のボタンの三角")]
    [SerializeField] private Graphic increaseArrow;

    [Header("色と大きさ")]
    [Tooltip("ON の色")]
    [SerializeField] private Color onColor = new Color(0.36f, 0.78f, 1f);

    [Tooltip("OFF の色")]
    [SerializeField] private Color offColor = new Color(0.54f, 0.56f, 0.62f);

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
    private float decreaseHitAt = float.NegativeInfinity;
    private float increaseHitAt = float.NegativeInfinity;

    public event Action<bool> Changed;

    public bool Value => value;

    private void Awake()
    {
        decreaseButton.onClick.AddListener(() => Step(-1));
        increaseButton.onClick.AddListener(() => Step(1));
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
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
    }

    public override void Step(int direction)
    {
        if (direction == 0)
            return;

        if (direction < 0)
            decreaseHitAt = Time.unscaledTime;
        else
            increaseHitAt = Time.unscaledTime;

        value = !value;
        Refresh();
        Changed?.Invoke(value);
    }

    private void Refresh()
    {
        valueText.text = value ? onText : offText;
        valueText.color = value ? onColor : offColor;
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
