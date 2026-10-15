using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
 
/// <summary>
/// アイテムスロットのUI表示コンポーネント。
/// インデックスでアイテムを識別し、InventoryUIから表示更新・ハイライトを受け取る。
/// </summary>
public class ItemSlotView : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image Button;
    [SerializeField] private TMP_Text itemNameText;
    [Tooltip("右上に出すレベル。レベルの無いアイテムでは隠す")]
    [SerializeField] private TMP_Text levelText;
    [Tooltip("左上に並べる属性のバッジ")]
    [SerializeField] private AttributeBadgeListView attributeBadges;

    /// <summary>Items リスト上のインデックス。InventoryUI が割り当てる。</summary>
    public int ItemIndex { get; private set; } = -1;

    public event Action<AttributeDefinition> AttributeClicked;

    private void Awake()
    {
        attributeBadges.BadgeClicked += attribute => AttributeClicked?.Invoke(attribute);
    }

    /// <summary>
    /// 表示内容とインデックスを更新する
    /// </summary>
    public void Refresh(OwnedItem owned, int index)
    {
        ItemIndex = index;

        if (owned == null)
        {
            itemIcon.sprite  = null;
            itemIcon.gameObject.SetActive(false);
            itemNameText.text = "Empty";
            levelText.gameObject.SetActive(false);
            return;
        }

        Item item = owned.Item;
        itemIcon.sprite   = item.Icon;
        itemIcon.gameObject.SetActive(true);
        itemNameText.text = item.DisplayName;
        levelText.gameObject.SetActive(item.MaxLevel > 1);
        levelText.text = $"Lv{owned.Level + 1}";
    }

    public void ShowAttributes(OwnedItem owned, AttributeSnapshot snapshot)
    {
        bool hasItem = owned != null && owned.Item != null;
        attributeBadges.Show(hasItem ? owned.Item.Attributes : Array.Empty<AttributeDefinition>(), snapshot);
    }
 
    /// <summary>
    /// 選択状態の表示切り替え
    /// </summary>
    public void SetHighlight(bool highlight)
    {
        Button.color = highlight ? Color.yellow : Color.white;
    }
}