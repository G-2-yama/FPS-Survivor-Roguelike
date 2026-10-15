using System.Collections.Generic;

public class AttributeSnapshot
{
    private readonly Dictionary<AttributeDefinition, int> counts;
    private readonly Dictionary<string, int> weaponLevels = new Dictionary<string, int>();
    private readonly HashSet<string> itemIds = new HashSet<string>();
    private readonly HashSet<string> addedWeaponIds;
    private readonly HashSet<string> addedItemIds;

    public AttributeCatalog Catalog { get; }

    public AttributeSnapshot(
        IEnumerable<(WeaponData data, int level)> weapons, IEnumerable<Item> items, AttributeCatalog catalog,
        HashSet<string> addedWeaponIds = null, HashSet<string> addedItemIds = null)
    {
        Catalog = catalog;
        this.addedWeaponIds = addedWeaponIds ?? new HashSet<string>();
        this.addedItemIds = addedItemIds ?? new HashSet<string>();

        var weaponList = new List<(WeaponData data, int level)>(weapons);
        var itemList = new List<Item>(items);
        counts = AttributeTracker.Count(weaponList, itemList, catalog);

        foreach ((WeaponData data, int level) in weaponList)
        {
            if (data == null || data.IsEmpty)
                continue;

            string id = AttributeTracker.IdOf(data);
            if (!weaponLevels.TryGetValue(id, out int current) || current < level)
                weaponLevels[id] = level;
        }

        foreach (Item item in itemList)
        {
            if (item != null)
                itemIds.Add(AttributeTracker.IdOf(item));
        }
    }

    public int GetCount(AttributeDefinition attribute)
    {
        return attribute != null && counts.TryGetValue(attribute, out int count) ? count : 0;
    }

    public bool TryGetLevel(WeaponData data, out int level)
    {
        level = 0;
        return data != null && weaponLevels.TryGetValue(AttributeTracker.IdOf(data), out level);
    }

    public bool Has(Item item) => item != null && itemIds.Contains(AttributeTracker.IdOf(item));

    public bool IsAdded(WeaponData data) => data != null && addedWeaponIds.Contains(AttributeTracker.IdOf(data));

    public bool IsAdded(Item item) => item != null && addedItemIds.Contains(AttributeTracker.IdOf(item));
}
