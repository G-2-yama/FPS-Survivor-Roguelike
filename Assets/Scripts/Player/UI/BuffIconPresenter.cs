using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>HPバー上のバフ一覧を管理する。効果の適用や期限の更新は行わない。</summary>
[DisallowMultipleComponent]
public sealed class BuffIconPresenter : MonoBehaviour
{
    [SerializeField, Min(0f)] private float blinkBeforeSeconds = 1f;
    [SerializeField, Min(0.05f)] private float blinkPeriod = 0.5f;
    [SerializeField, Min(8f)] private float iconSize = 32f;
    [SerializeField, Min(0f)] private float spacing = 6f;
    [SerializeField] private float offsetAboveBar = 12f;
    private TimedBuffManager manager;
    private RectTransform row;
    private readonly Dictionary<TimedBuff, BuffIconView> views = new Dictionary<TimedBuff, BuffIconView>();

    public void Initialize(TimedBuffManager buffManager, RectTransform healthBar)
    {
        if (manager != null) manager.BuffsChanged -= Refresh;
        ClearViews();
        manager = buffManager;
        if (row != null) Destroy(row.gameObject);
        row = new GameObject("BuffIcons", typeof(RectTransform)).GetComponent<RectTransform>();
        row.SetParent(healthBar, false);
        row.gameObject.layer = healthBar.gameObject.layer;
        row.anchorMin = row.anchorMax = new Vector2(0f, 1f);
        row.pivot = Vector2.zero;
        row.anchoredPosition = new Vector2(0f, offsetAboveBar);
        row.sizeDelta = new Vector2(healthBar.rect.width, iconSize);
        if (isActiveAndEnabled) Subscribe();
    }

    private void Subscribe()
    {
        if (manager == null || row == null) return;
        manager.BuffsChanged -= Refresh;
        manager.BuffsChanged += Refresh;
        Refresh();
    }
    private void OnEnable() => Subscribe();
    private void OnDisable()
    {
        if (manager != null) manager.BuffsChanged -= Refresh;
        ClearViews();
    }
    private void OnDestroy()
    {
        if (row != null) Destroy(row.gameObject);
    }
    private void ClearViews()
    {
        foreach (var view in views.Values) if (view != null) { view.gameObject.SetActive(false); Destroy(view.gameObject); }
        views.Clear();
    }
    private void Refresh()
    {
        var removed = new List<TimedBuff>();
        foreach (var pair in views)
        {
            bool found = false;
            foreach (var buff in manager.ActiveBuffs) if (ReferenceEquals(buff, pair.Key)) { found = true; break; }
            if (!found) removed.Add(pair.Key);
        }
        foreach (var buff in removed) { views[buff].gameObject.SetActive(false); Destroy(views[buff].gameObject); views.Remove(buff); }
        int index = 0;
        foreach (var buff in manager.ActiveBuffs)
        {
            if (buff.Definition == null || buff.Definition.Icon == null) continue;
            if (!views.TryGetValue(buff, out var view))
            {
                var go = new GameObject(buff.Definition.DisplayName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(BuffIconView));
                go.transform.SetParent(row, false);
                go.layer = row.gameObject.layer;
                view = go.GetComponent<BuffIconView>();
                view.Initialize(buff.Definition.Icon);
                views.Add(buff, view);
            }
            var rect = (RectTransform)view.transform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.sizeDelta = new Vector2(iconSize, iconSize);
            rect.anchoredPosition = new Vector2(index++ * (iconSize + spacing), 0f);
            view.Render(buff.RemainingTime, 0f, blinkBeforeSeconds, Mathf.Max(0.05f, blinkPeriod));
        }
    }
    private void Update()
    {
        foreach (var pair in views)
            pair.Value.Render(pair.Key.RemainingTime, Time.deltaTime, blinkBeforeSeconds, Mathf.Max(0.05f, blinkPeriod));
    }
}