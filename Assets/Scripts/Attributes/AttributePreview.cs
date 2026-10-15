using System.Collections.Generic;

public readonly struct AttributeChange
{
    public readonly AttributeDefinition Attribute;
    public readonly int Before;
    public readonly int After;

    public AttributeChange(AttributeDefinition attribute, int before, int after)
    {
        Attribute = attribute;
        Before = before;
        After = after;
    }
}

public class AttributePreview
{
    private readonly AttributeTracker current;
    private readonly List<(WeaponData data, int level)> weapons;
    private readonly List<Item> items;
    private readonly int ownedWeaponCount;
    private readonly int ownedItemCount;
    private readonly List<AttributeDefinition> shown = new List<AttributeDefinition>();
    private AttributeSnapshot after;

    public AttributePreview(PlayerInventory inventory, AttributeTracker current)
    {
        this.current = current;
        weapons = new List<(WeaponData data, int level)>(AttributeTracker.OwnedWeapons(inventory));
        items = new List<Item>(AttributeTracker.OwnedItems(inventory));
        ownedWeaponCount = weapons.Count;
        ownedItemCount = items.Count;
    }

    public AttributeSnapshot After => after ??= CreateSnapshot();

    public void AddWeapon(WeaponData data, int level)
    {
        if (data == null || data.IsEmpty)
            return;

        weapons.Add((data, level));
        after = null;
        Show(data.Attributes);
    }

    public void AddItem(Item item)
    {
        if (item == null)
            return;

        items.Add(item);
        after = null;
        Show(item.Attributes);

        if (item.CountsAsAllBases && current.Catalog != null)
        {
            foreach (AttributeDefinition attribute in current.Catalog.All)
            {
                if (attribute != null && attribute.Kind == AttributeKind.Base)
                    Show(attribute);
            }
        }
    }

    public List<AttributeChange> GetChanges()
    {
        var changes = new List<AttributeChange>(shown.Count);
        foreach (AttributeDefinition attribute in shown)
            changes.Add(new AttributeChange(attribute, current.GetCount(attribute), After.GetCount(attribute)));
        return changes;
    }

    private AttributeSnapshot CreateSnapshot()
    {
        var ownedWeaponIds = new HashSet<string>();
        for (int i = 0; i < ownedWeaponCount; i++)
            ownedWeaponIds.Add(AttributeTracker.IdOf(weapons[i].data));

        var ownedItemIds = new HashSet<string>();
        for (int i = 0; i < ownedItemCount; i++)
            ownedItemIds.Add(AttributeTracker.IdOf(items[i]));

        var addedWeaponIds = new HashSet<string>();
        for (int i = ownedWeaponCount; i < weapons.Count; i++)
        {
            string id = AttributeTracker.IdOf(weapons[i].data);
            if (!ownedWeaponIds.Contains(id))
                addedWeaponIds.Add(id);
        }

        var addedItemIds = new HashSet<string>();
        for (int i = ownedItemCount; i < items.Count; i++)
        {
            string id = AttributeTracker.IdOf(items[i]);
            if (!ownedItemIds.Contains(id))
                addedItemIds.Add(id);
        }

        return new AttributeSnapshot(weapons, items, current.Catalog, addedWeaponIds, addedItemIds);
    }

    private void Show(IReadOnlyList<AttributeDefinition> attributes)
    {
        foreach (AttributeDefinition attribute in attributes)
            Show(attribute);
    }

    private void Show(AttributeDefinition attribute)
    {
        if (attribute != null && !shown.Contains(attribute))
            shown.Add(attribute);
    }
}
