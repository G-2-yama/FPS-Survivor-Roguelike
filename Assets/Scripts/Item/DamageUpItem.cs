using UnityEngine;


[CreateAssetMenu(menuName = "Items/damageUp")]
public class DamageUpItem : Item
{
    [SerializeField] private float DamageIncreaseAmount = 0.1f;

    public override bool IsAvailable()
    {
        return true;
    }

    public override void Apply()
    {
        player.Stats.AddDamageMultiplier(DamageIncreaseAmount);
    }
    
    public override void Revert()
    {
        player.Stats.AddDamageMultiplier(-DamageIncreaseAmount);
    }
}
