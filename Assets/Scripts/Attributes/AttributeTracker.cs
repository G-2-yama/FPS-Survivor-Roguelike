using System;
using System.Collections.Generic;

public class AttributeTracker
{
    private readonly PlayerInventory inventory;
    private readonly AttributeCatalog catalog;
    private Dictionary<AttributeDefinition, int> counts = new Dictionary<AttributeDefinition, int>();

    public AttributeCatalog Catalog => catalog;

    public event Action Changed;

    public AttributeTracker(PlayerInventory inventory, AttributeCatalog catalog)
    {
        this.inventory = inventory;
        this.catalog = catalog;
        inventory.OnSlotChanged += HandleSlotChanged;
        inventory.OnItemsChanged += Recount;
    }

    public void Dispose()
    {
        inventory.OnSlotChanged -= HandleSlotChanged;
        inventory.OnItemsChanged -= Recount;
    }

    public int GetCount(AttributeDefinition attribute)
    {
        return attribute != null && counts.TryGetValue(attribute, out int count) ? count : 0;
    }

    public bool IsActive(AttributeDefinition attribute) => attribute != null && attribute.IsActiveAt(GetCount(attribute));

    public bool IsMax(AttributeDefinition attribute) => attribute != null && attribute.IsMaxAt(GetCount(attribute));

    public bool IsConditionMet(AttributeDefinition attribute) => attribute != null && attribute.IsConditionMetAt(GetCount(attribute));

    public void Recount()
    {
        counts = Count(OwnedWeapons(inventory), OwnedItems(inventory), catalog);
        Changed?.Invoke();
    }

    private void HandleSlotChanged(SlotType slot, Weapon weapon) => Recount();

    public static IEnumerable<(WeaponData data, int level)> OwnedWeapons(PlayerInventory inventory)
    {
        if (inventory.WeaponSlots == null)
            yield break;

        foreach (Weapon weapon in inventory.WeaponSlots.Values)
        {
            if (weapon != null && weapon.HasWeapon)
                yield return (weapon.WeaponData, weapon.Level);
        }
    }

    public static IEnumerable<Item> OwnedItems(PlayerInventory inventory)
    {
        foreach (OwnedItem owned in inventory.Items)
        {
            if (owned != null && owned.Item != null)
                yield return owned.Item;
        }
    }

    public static Dictionary<AttributeDefinition, int> Count(
        IEnumerable<(WeaponData data, int level)> weapons, IEnumerable<Item> items, AttributeCatalog catalog)
    {
        var weightById = new Dictionary<string, (WeaponData data, int weight)>();
        foreach ((WeaponData data, int level) in weapons)
        {
            if (data == null || data.IsEmpty)
                continue;

            string id = IdOf(data);
            int weight = data.WeaponType == WeaponType.Main && data.IsEvoLevel(level) ? 2 : 1;
            if (!weightById.TryGetValue(id, out var current) || current.weight < weight)
                weightById[id] = (data, weight);
        }

        var result = new Dictionary<AttributeDefinition, int>();
        foreach ((WeaponData data, int weight) in weightById.Values)
        {
            AddAll(result, data.Attributes, weight);
        }

        var seenItems = new HashSet<string>();
        bool countsAsAllBases = false;
        foreach (Item item in items)
        {
            if (item == null || !seenItems.Add(IdOf(item)))
                continue;

            AddAll(result, item.Attributes, 1);
            countsAsAllBases |= item.CountsAsAllBases;
        }

        if (countsAsAllBases && catalog != null)
        {
            foreach (AttributeDefinition attribute in catalog.All)
            {
                if (attribute != null && attribute.Kind == AttributeKind.Base)
                    Add(result, attribute, 1);
            }
        }

        return result;
    }

    public static string IdOf(WeaponData data) => string.IsNullOrEmpty(data.WeaponId) ? data.name : data.WeaponId;

    public static string IdOf(Item item) => string.IsNullOrEmpty(item.ItemId) ? item.name : item.ItemId;

    private static void AddAll(Dictionary<AttributeDefinition, int> counts, IReadOnlyList<AttributeDefinition> attributes, int amount)
    {
        var seen = new HashSet<AttributeDefinition>();
        foreach (AttributeDefinition attribute in attributes)
        {
            if (attribute != null && seen.Add(attribute))
                Add(counts, attribute, amount);
        }
    }

    private static void Add(Dictionary<AttributeDefinition, int> counts, AttributeDefinition attribute, int amount)
    {
        counts.TryGetValue(attribute, out int count);
        counts[attribute] = count + amount;
    }
}
