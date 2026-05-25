using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeView : MonoBehaviour
{
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private UpgradeButtonView[] upgradeButtonsViews;

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
        upgradeCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// アップグレードUIを非表示にする
    /// </summary>
    public void Hide()
    {
        upgradeCanvas.gameObject.SetActive(false);
    }

}