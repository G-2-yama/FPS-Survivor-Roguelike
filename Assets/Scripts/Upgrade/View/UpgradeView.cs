using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class UpgradeView : MonoBehaviour
{
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private UpgradeButtonView[] upgradeButtonsViews;
    [SerializeField] private UIFader uiFader;
    [SerializeField, Min(0.01f)] private float cardFadeDuration = 0.25f;
    [SerializeField, Min(0f)] private float cardFadeInterval = 0.1f;
    [SerializeField, Min(0f)] private float cardSlideDistance = 60f;

    private CancellationTokenSource revealCancellation;

    /// <summary>
    /// 各カードの表示に必要な情報を初期化する
    /// </summary>
    private void Awake()
    {
        foreach (var buttonView in upgradeButtonsViews)
        {
            buttonView.Initialize();
        }
    }

    /// <summary>
    /// アップグレードの選択肢をセットアップする
    /// </summary>
    /// <param name="choices">選択肢のリスト</param>
    public void Setup(List<UpgradeBase> choices)
    {
        for (int i = 0; i < upgradeButtonsViews.Length; i++)
        {
            if (i < choices.Count)
            {
                upgradeButtonsViews[i].gameObject.SetActive(true);
                upgradeButtonsViews[i].Setup(choices[i]);
            }
            else
            {
                upgradeButtonsViews[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// アップグレードUIを表示する
    /// </summary>
    public void Show()
    {
        // カードのない初期状態に戻す
        StopReveal();
        foreach (var buttonView in upgradeButtonsViews)
        {
            buttonView.SetRevealState(0f, false, cardSlideDistance);
        }

        // UIの表示
        upgradeCanvas.gameObject.SetActive(true);
        uiFader.FadeIn();

        // 表示処理の開始
        revealCancellation = new CancellationTokenSource();
        RevealCardsAsync(revealCancellation).Forget();
    }

    /// <summary>
    /// アップグレードUIを非表示にする
    /// </summary>
    public void Hide()
    {
        StopReveal();
        foreach (var buttonView in upgradeButtonsViews)
        {
            buttonView.SetRevealInteractable(false);
        }

        uiFader.FadeOut(() =>
        {
            upgradeCanvas.gameObject.SetActive(false);
        });
    }

    /// <summary>
    /// カードの表示処理を実行し、キャンセルとリソースの解放を管理する
    /// </summary>
    /// <param name="cancellation">今回の表示処理で使用するキャンセル管理オブジェクト</param>
    /// <returns>表示処理の終了とリソースの解放を待機するタスク</returns>
    private async UniTask RevealCardsAsync(CancellationTokenSource cancellation)
    {
        try
        {
            // 配列は画面の左から右の順。非表示の選択肢は待ち時間に含めない。
            var visibleCards = new List<UpgradeButtonView>();
            foreach (var buttonView in upgradeButtonsViews)
            {
                if (buttonView.gameObject.activeSelf)
                    visibleCards.Add(buttonView);
            }

            // カード表示処理
            float duration = Mathf.Max(0.01f, cardFadeDuration);
            float interval = Mathf.Max(0f, cardFadeInterval);
            for (int i = 0; i < visibleCards.Count; i++)
            {
                await visibleCards[i].RevealAsync(duration, cardSlideDistance, cancellation.Token);
                if (i < visibleCards.Count - 1 && interval > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(interval), ignoreTimeScale: true, cancellationToken: cancellation.Token);
                }
            }

            // 中断されていないなら，カードを選択可能にする
            cancellation.Token.ThrowIfCancellationRequested();
            foreach (var buttonView in visibleCards)
            {
                buttonView.SetRevealInteractable(true);
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {

        }
        finally
        {
            if (revealCancellation == cancellation)
                revealCancellation = null;
            cancellation.Dispose();
        }
    }

    /// <summary>
    /// 実行中のカードの表示処理を中断する
    /// </summary>
    private void StopReveal()
    {
        var cancellation = revealCancellation;
        revealCancellation = null;
        cancellation?.Cancel();
    }

    /// <summary>
    /// オブジェクトが無効になったときにカードの表示処理を中断する
    /// </summary>
    private void OnDisable()
    {
        StopReveal();
    }
}
