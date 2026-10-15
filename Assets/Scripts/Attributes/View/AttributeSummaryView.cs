using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttributeSummaryView : MonoBehaviour
{
    [Tooltip("属性1つ分（バッジと数）のプレハブ")]
    [SerializeField] private AttributeCountView itemPrefab;
    [Tooltip("並びの幅の上限。超えたら全体を縮めて収める。0 なら縮めない")]
    [SerializeField, Min(0f)] private float maxWidth = 1420f;

    private readonly List<AttributeCountView> items = new List<AttributeCountView>();

    public event Action<AttributeDefinition> BadgeClicked;

    public void Show(AttributeSnapshot snapshot)
    {
        int used = 0;
        if (snapshot.Catalog != null)
        {
            foreach (AttributeDefinition attribute in snapshot.Catalog.All)
            {
                int count = snapshot.GetCount(attribute);
                if (attribute == null || count <= 0)
                    continue;

                if (used == items.Count)
                    items.Add(CreateItem());

                AttributeCountView item = items[used++];
                item.gameObject.SetActive(true);
                item.Setup(attribute, count);
            }
        }

        for (int i = used; i < items.Count; i++)
            items[i].gameObject.SetActive(false);

        FitWidth();
    }

    private AttributeCountView CreateItem()
    {
        AttributeCountView item = Instantiate(itemPrefab, transform);
        item.BadgeClicked += attribute => BadgeClicked?.Invoke(attribute);
        return item;
    }

    private void FitWidth()
    {
        var rect = (RectTransform)transform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        float width = LayoutUtility.GetPreferredWidth(rect);
        float scale = maxWidth > 0f && width > maxWidth ? maxWidth / width : 1f;
        rect.localScale = new Vector3(scale, scale, 1f);
    }
}
