public class OwnedItem
{
    public Item Item { get; }
    public int Level { get; internal set; }

    public OwnedItem(Item item, int level)
    {
        Item = item;
        Level = level;
    }

    public bool CanLevelUp => Level < Item.MaxLevel - 1;
}
