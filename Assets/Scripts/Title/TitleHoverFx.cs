using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// タイトル画面のボタンにホバー演出を適用する
/// </summary>
public class TitleHoverFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Serializable]
    private class FlashBar
    {
        [Tooltip("ホバーした瞬間に走る横帯")]
        [SerializeField] private Graphic bar;

        [Tooltip("ホバーしてから走り出すまでの時間（秒）")]
        [SerializeField] private float delay;

        public void Apply(float elapsed, float duration)
        {
            float t = (elapsed - delay) / duration;
            float alpha = 0f;
            float shift = 0f;
            if (t >= 0f && t < 0.25f)
            {
                alpha = 0.7f;
                shift = -0.3f;
            }
            else if (t >= 0.5f && t < 0.75f)
            {
                alpha = 0.5f;
                shift = 0.2f;
            }

            Color color = bar.color;
            color.a = alpha;
            bar.color = color;
            bar.rectTransform.anchoredPosition = new Vector2(shift * bar.rectTransform.rect.width, bar.rectTransform.anchoredPosition.y);
        }
    }

    [Header("動き")]
    [Tooltip("ホバー中にずらす中身")]
    [SerializeField] private RectTransform body;

    [Tooltip("ホバー中にずらす量（px）")]
    [SerializeField] private Vector2 hoverOffset = new Vector2(24f, 0f);

    [Tooltip("ホバーで縦に伸びるバー 未設定でも動作します")]
    [SerializeField] private RectTransform growY;

    [Tooltip("ホバーで横に伸びる線 未設定でも動作します")]
    [SerializeField] private RectTransform growX;

    [Tooltip("ホバーの切り替えにかける時間（秒）")]
    [SerializeField] private float duration = 0.2f;

    [Header("色")]
    [Tooltip("ホバー中・選択中に色を変える対象")]
    [SerializeField] private List<TitleColorTarget> colors = new List<TitleColorTarget>();

    [Header("グリッチ")]
    [Tooltip("ホバー中に色ずれさせる文字")]
    [SerializeField] private List<TitleRgbText> rgbTexts = new List<TitleRgbText>();

    [Tooltip("ホバーした瞬間に走る横帯")]
    [SerializeField] private List<FlashBar> flashBars = new List<FlashBar>();

    [Tooltip("横帯が走る時間（秒）")]
    [SerializeField] private float flashDuration = 0.4f;

    public bool Highlighted { get; set; }

    /// <summary>
    /// 指定した描画対象の通常色とホバー色を切り替える。
    /// </summary>
    public void SetGraphicColors(Graphic graphic, Color normal, Color active)
    {
        foreach (TitleColorTarget target in colors)
        {
            if (target.TrySetColors(graphic, normal, active))
                break;
        }

        Apply();
    }

    private Vector2 bodyHome;
    private bool isHovered;
    private bool isFocused;
    private float enteredAt;
    private float hoverProgress;
    private float colorProgress;

    private void Awake()
    {
        if (body != null)
            bodyHome = body.anchoredPosition;
        Apply();
    }

    /// <summary>
    /// ホバーしたボタンをゲームパッド決定の対象にする
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetActiveState(true, isFocused);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetActiveState(false, isFocused);
    }

    public bool Focused
    {
        get => isFocused;
        set => SetActiveState(isHovered, value);
    }

    private bool IsActive => isHovered || isFocused;

    private void SetActiveState(bool hovered, bool focused)
    {
        bool wasActive = IsActive;
        isHovered = hovered;
        isFocused = focused;
        if (IsActive == wasActive)
            return;

        if (IsActive)
        {
            enteredAt = Time.unscaledTime;
            foreach (var rgbText in rgbTexts)
                rgbText.Play();
        }
        else
        {
            foreach (var rgbText in rgbTexts)
                rgbText.Stop();
        }
    }

    private void OnDisable()
    {
        SetActiveState(false, false);

        hoverProgress = 0f;
        colorProgress = Highlighted ? 1f : 0f;
        Apply();
    }

    private void Update()
    {
        float step = Time.unscaledDeltaTime / duration;
        hoverProgress = Mathf.MoveTowards(hoverProgress, IsActive ? 1f : 0f, step);
        colorProgress = Mathf.MoveTowards(colorProgress, IsActive || Highlighted ? 1f : 0f, step);
        Apply();
    }

    private void Apply()
    {
        float hover = TitleEase.Ease(hoverProgress);
        if (body != null)
            body.anchoredPosition = bodyHome + hoverOffset * hover;
        if (growY != null)
            growY.localScale = new Vector3(1f, hover, 1f);
        if (growX != null)
            growX.localScale = new Vector3(hover, 1f, 1f);

        float color = TitleEase.Ease(colorProgress);
        foreach (var target in colors)
            target.Apply(color);

        float elapsed = IsActive ? Time.unscaledTime - enteredAt : float.NegativeInfinity;
        foreach (var flashBar in flashBars)
            flashBar.Apply(elapsed, flashDuration);
    }
}
