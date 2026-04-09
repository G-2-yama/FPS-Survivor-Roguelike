using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButtonView : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Text buttonText;

    private UpgradeBase upgrade;
    private UpgradeView view;

    public void Setup(UpgradeBase upgrade, UpgradeView view)
    {
        this.upgrade = upgrade;
        this.view = view;

        buttonText.text = upgrade.DisplayName;
    }

    /// <summary>
    /// マウスオーバーでアップグレードの詳細を表示する
    /// </summary>
    /// <param name="eventData">ポインタイベントデータ</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        view.ShowUpgradeDetail(upgrade);
    }

    /// <summary>
    /// マウスが離れたらアップグレードの詳細を非表示にする
    /// </summary>
    /// <param name="eventData">ポインタイベントデータ</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        view.HideUpgradeDetail();
    }
}
