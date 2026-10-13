using UnityEngine;
[CreateAssetMenu(menuName = "Items/Expcatcher")]
public class Expcatcher : Item
{

    [SerializeField] private float increaseamount = 5;

    public override bool IsAvailable()
    {
        return true;
    }

    public override void Apply()
    {
        player.Stats.AddPickupRadius(increaseamount);
    }

    public override void Revert()
    {
       player.Stats.AddPickupRadius(-increaseamount);
    }
}
