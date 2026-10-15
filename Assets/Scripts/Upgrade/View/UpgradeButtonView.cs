using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;

public class UpgradeButtonView : MonoBehaviour
{
    [SerializeField] Image ButtonImage;
    [SerializeField] Image FrameImage;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image iconImage;
    [Tooltip("名前の下に出すレベル。レベルの無い候補では隠す")]
    [SerializeField] private TMP_Text levelText;
    [Tooltip("Evo などの色")]
    [SerializeField] private UIPalette palette;

    [Header("Attributes")]
    [Tooltip("属性の行のプレハブ")]
    [SerializeField] private AttributeRowView attributeRowPrefab;
    [Tooltip("属性の行を並べる親。説明の文のすぐ下に置き直す")]
    [SerializeField] private RectTransform attributeRowContainer;
    [Tooltip("説明の文と属性の行の間")]
    [SerializeField, Min(0f)] private float attributeRowGap = 28f;

    private readonly List<AttributeRowView> attributeRows = new List<AttributeRowView>();
    private AttributeSnapshot attributeSnapshot;
    private AttributeDetailPanel attributeDetailPanel;
    private RectTransform descriptionRect;
    private Vector2 descriptionOffsetMin;

    private CanvasGroup revealCanvasGroup;
    private RectTransform revealRectTransform;
    private Vector2 revealTargetPosition;

    /// <summary>
    /// カードの表示に必要なコンポーネントと元の位置を初期化する
    /// </summary>
    public void Initialize()
    {
        revealRectTransform = GetComponent<RectTransform>();
        revealTargetPosition = revealRectTransform.anchoredPosition;
        revealCanvasGroup = GetComponent<CanvasGroup>();
        if (revealCanvasGroup == null)
            revealCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        descriptionRect = descriptionText.rectTransform;
        descriptionOffsetMin = descriptionRect.offsetMin;
    }

    /// <summary>
    /// カードを下から元の位置へ移動させながらフェードインする
    /// </summary>
    /// <param name="duration">表示アニメーションの再生時間（秒）</param>
    /// <param name="slideDistance">表示開始位置を下にずらす距離（UI座標）</param>
    /// <param name="cancellationToken">表示アニメーションを中断するためのトークン</param>
    /// <returns>カードの表示完了を待機するタスク</returns>
    public async UniTask RevealAsync(float duration, float slideDistance, CancellationToken cancellationToken)
    {
        // カードのアニメーション
        duration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetRevealState(elapsed / duration, false, slideDistance);
            await UniTask.NextFrame(cancellationToken: cancellationToken);
        }

        // アニメーションの最終状態を設定する
        SetRevealState(1f, false, slideDistance);
    }

    /// <summary>
    /// 表示の進行度に応じてカードの位置、透明度、操作可否を更新する
    /// </summary>
    /// <param name="progress">表示の進行度（0で非表示、1で表示完了）</param>
    /// <param name="interactable">カードを操作可能にするか</param>
    /// <param name="slideDistance">表示開始位置を下にずらす距離（UI座標）</param>
    public void SetRevealState(float progress, bool interactable, float slideDistance = 0f)
    {
        progress = Mathf.Clamp01(progress);
        float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
        revealRectTransform.anchoredPosition = revealTargetPosition + Vector2.down * (Mathf.Max(0f, slideDistance) * (1f - easedProgress));
        revealCanvasGroup.alpha = progress;
        SetRevealInteractable(interactable);
    }

    /// <summary>
    /// カードの操作とポインター入力の受け付けを切り替える
    /// </summary>
    /// <param name="interactable">カードを操作可能にするか</param>
    public void SetRevealInteractable(bool interactable)
    {
        revealCanvasGroup.interactable = interactable;
        revealCanvasGroup.blocksRaycasts = interactable;
    }

    /// <summary>
    /// アップグレードの名前、説明、アイコン、レアリティの色を設定する
    /// </summary>
    /// <param name="upgrade">表示するアップグレード</param>
    public void Setup(UpgradeBase upgrade, AttributePreview attributePreview, AttributeDetailPanel detailPanel)
    {
        attributeSnapshot = attributePreview?.After;
        attributeDetailPanel = detailPanel;
        buttonText.text = upgrade.DisplayName;
        descriptionText.text = upgrade.Description;
        iconImage.sprite = upgrade.Icon;

        UpgradeLevelStep? step = upgrade.LevelStep;
        levelText.gameObject.SetActive(step.HasValue);
        if (step.HasValue)
            levelText.text = FormatLevel(step.Value);

        ButtonImage.color = upgrade.Rarity.GetColor(0.1f);
        FrameImage.color = upgrade.Rarity.GetColor(1f);

        SetupAttributeRows(attributePreview != null ? attributePreview.GetChanges() : new List<AttributeChange>());
    }

    private void SetupAttributeRows(IReadOnlyList<AttributeChange> changes)
    {
        while (attributeRows.Count < changes.Count)
        {
            AttributeRowView row = Instantiate(attributeRowPrefab, attributeRowContainer);
            row.BadgeClicked += OpenAttributeDetail;
            attributeRows.Add(row);
        }

        for (int i = 0; i < attributeRows.Count; i++)
        {
            bool used = i < changes.Count;
            attributeRows[i].gameObject.SetActive(used);
            if (used)
                attributeRows[i].Setup(changes[i].Attribute, changes[i].Before, changes[i].After);
        }

        attributeRowContainer.gameObject.SetActive(changes.Count > 0);
    }

    private void OpenAttributeDetail(AttributeDefinition attribute)
    {
        if (attributeDetailPanel != null)
            attributeDetailPanel.Show(attribute, attributeSnapshot);
    }

    public void LayoutAttributeRows()
    {
        descriptionRect.offsetMin = descriptionOffsetMin;
        if (!attributeRowContainer.gameObject.activeSelf)
            return;

        LayoutRebuilder.ForceRebuildLayoutImmediate(attributeRowContainer);
        float rowsHeight = attributeRowContainer.rect.height;
        descriptionRect.offsetMin = descriptionOffsetMin + Vector2.up * (rowsHeight + attributeRowGap);

        descriptionText.ForceMeshUpdate(true);
        float textHeight = string.IsNullOrEmpty(descriptionText.text) ? 0f : descriptionText.GetRenderedValues(true).y;
        if (float.IsNaN(textHeight) || textHeight < 0f)
            textHeight = 0f;

        float descriptionTop = descriptionRect.localPosition.y + descriptionRect.rect.yMax;
        Vector3 position = attributeRowContainer.localPosition;
        position.y = descriptionTop - textHeight - attributeRowGap - attributeRowContainer.rect.yMax;
        attributeRowContainer.localPosition = position;
    }

    private string FormatLevel(UpgradeLevelStep step)
    {
        string next = step.NextIsEvo ? palette.ColorizeEvo("Evo") : step.Next.ToString();
        return step.Current > 0 ? $"Lv{step.Current} → {next}" : $"Lv{next}";
    }
}
