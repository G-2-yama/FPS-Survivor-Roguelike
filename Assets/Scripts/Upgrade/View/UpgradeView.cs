using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeView : MonoBehaviour
{
    [SerializeField] private Canvas upgradeCanvas;
    [SerializeField] private UpgradeButtonView[] upgradeButtonsViews;
    [SerializeField] private Text displayNameText;
    [SerializeField] private Text descriptionText;

    /// <summary>
    /// アップグレードの選択肢をセットアップする
    /// </summary>
    /// <param name="choices">選択肢のリスト</param>
    /// <param name="manager">アップグレードマネージャ</param>
    public void Setup(List<UpgradeBase> choices)
    {
        for (int i = 0; i < upgradeButtonsViews.Length; i++)
        {
            if (i < choices.Count)
            {
                upgradeButtonsViews[i].gameObject.SetActive(true);
                upgradeButtonsViews[i].Setup(choices[i], this);
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

    /// <summary>
    /// アップグレードの詳細を表示する
    /// </summary>
    /// <param name="upgrade">表示するアップグレード</param>
    public void ShowUpgradeDetail(UpgradeBase upgrade)
    {
        displayNameText.text = upgrade.DisplayName;
        descriptionText.text = upgrade.Description;
    }

    /// <summary>
    /// アップグレードの詳細を非表示にする
    /// </summary>
    public void HideUpgradeDetail()
    {
        displayNameText.text = "";
        descriptionText.text = "";
    }

}