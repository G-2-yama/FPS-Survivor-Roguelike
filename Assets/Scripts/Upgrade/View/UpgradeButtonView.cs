using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;

public class UpgradeButtonView : MonoBehaviour
{
    [SerializeField] Image ButtonImage;
    [SerializeField] Image FrameImage;
    [SerializeField] private Text buttonText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Image iconImage;

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
    public void Setup(UpgradeBase upgrade)
    {
        buttonText.text = upgrade.DisplayName;
        descriptionText.text = upgrade.Description;
        iconImage.sprite = upgrade.Icon;

        ButtonImage.color = upgrade.Rarity.GetColor(0.1f);
        FrameImage.color = upgrade.Rarity.GetColor(1f);
    }
}
