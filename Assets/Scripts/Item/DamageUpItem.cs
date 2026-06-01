using UnityEngine;


[CreateAssetMenu(menuName = "Items/damageUp")]
public class DamageUpItem : Item
{
    [SerializeField] private int DamageIncreaseAmount = 10;

    public override bool IsAvailable()
    {
        return true;
    }

    public override void Apply()
    {
        player.Stats.AddDamage(DamageIncreaseAmount);
    }
    
    public override void Revert()
    {
        player.Stats.AddDamage(-DamageIncreaseAmount);
    }
}
