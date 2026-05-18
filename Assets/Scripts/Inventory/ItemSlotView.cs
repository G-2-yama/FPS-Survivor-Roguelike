using UnityEngine;
using UnityEngine.UI;
 
/// <summary>
/// アイテムスロットのUI表示コンポーネント。
/// インデックスでアイテムを識別し、InventoryUIから表示更新・ハイライトを受け取る。
/// </summary>
public class ItemSlotView : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image Button;
    [SerializeField] private Text itemNameText;
 
    /// <summary>Items リスト上のインデックス。InventoryUI が割り当てる。</summary>
    public int ItemIndex { get; private set; } = -1;
 
    /// <summary>
    /// 表示内容とインデックスを更新する
    /// </summary>
    public void Refresh(Item item, int index)
    {
        ItemIndex = index;
 
        if (item == null)
        {
            itemIcon.sprite  = null;
            itemIcon.gameObject.SetActive(false);
            itemNameText.text = "Empty";
            return;
        }
 
        itemIcon.sprite   = item.Icon;
        itemIcon.gameObject.SetActive(true);
        itemNameText.text = item.DisplayName;
    }
 
    /// <summary>
    /// 選択状態の表示切り替え
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        Button.color = highlight ? Color.yellow : Color.white;
    }
}