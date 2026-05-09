using UnityEngine;
using UnityEngine.UI;
 
/// <summary>
/// アイテムスロットのUI表示コンポーネント。
/// インデックスでアイテムを識別し、InventoryUIから表示更新・ハイライトを受け取る。
/// </summary>
public class ItemSlotView : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
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
            itemNameText.text = "Empty";
            return;
        }
 
        itemIcon.sprite   = item.Icon;
        itemNameText.text = item.DisplayName;
    }
 
    /// <summary>
    /// 選択状態の表示切り替え
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        itemIcon.color = highlight ? Color.yellow : Color.white;
    }
}