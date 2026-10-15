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
    private readonly List<AttributeDefinition> shown = new List<AttributeDefinition>();

    public AttributePreview(PlayerInventory inventory, AttributeTracker current)
    {
        this.current = current;
        weapons = new List<(WeaponData data, int level)>(AttributeTracker.OwnedWeapons(inventory));
        items = new List<Item>(AttributeTracker.OwnedItems(inventory));
    }

    public void AddWeapon(WeaponData data, int level)
    {
        if (data == null || data.IsEmpty)
            return;

        weapons.Add((data, level));
        Show(data.Attributes);
    }

    public void AddItem(Item item)
    {
        if (item == null)
            return;

        items.Add(item);
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
        Dictionary<AttributeDefinition, int> after = AttributeTracker.Count(weapons, items, current.Catalog);
        var changes = new List<AttributeChange>(shown.Count);
        foreach (AttributeDefinition attribute in shown)
        {
            after.TryGetValue(attribute, out int count);
            changes.Add(new AttributeChange(attribute, current.GetCount(attribute), count));
        }
        return changes;
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
