using System;
using System.Collections.Generic;
using UnityEngine;

public class AttributeBadgeListView : MonoBehaviour
{
    [Tooltip("並べるバッジのプレハブ")]
    [SerializeField] private AttributeBadgeView badgePrefab;
    [Tooltip("バッジの幅を属性名に合わせる。オフならプレハブの最小幅のまま")]
    [SerializeField] private bool fitBadgeToName = true;

    private readonly List<AttributeBadgeView> badges = new List<AttributeBadgeView>();

    public event Action<AttributeDefinition> BadgeClicked;

    public void Show(IEnumerable<AttributeDefinition> attributes, AttributeSnapshot snapshot)
    {
        int used = 0;
        foreach (AttributeDefinition attribute in attributes)
        {
            if (attribute == null)
                continue;

            if (used == badges.Count)
                badges.Add(CreateBadge());

            AttributeBadgeView badge = badges[used++];
            badge.gameObject.SetActive(true);
            badge.Setup(attribute, AttributeDisplayStates.Of(attribute, snapshot.GetCount(attribute)));
            badge.SetClickable(true);
        }

        for (int i = used; i < badges.Count; i++)
            badges[i].gameObject.SetActive(false);
    }

    private AttributeBadgeView CreateBadge()
    {
        AttributeBadgeView badge = Instantiate(badgePrefab, transform);
        if (fitBadgeToName)
            badge.FitWidthToName();
        badge.Clicked += attribute => BadgeClicked?.Invoke(attribute);
        return badge;
    }
}
