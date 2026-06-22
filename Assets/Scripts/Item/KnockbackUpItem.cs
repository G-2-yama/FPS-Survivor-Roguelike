using UnityEngine;


[CreateAssetMenu(menuName = "Items/knockbackUp")]
public class KnockbackUpItem : Item
{
    [SerializeField] private float KnockbackIncreaseAmount = 0.1f;

    public override bool IsAvailable()
    {
        return true;
    }

    public override void Apply()
    {
        player.Stats.AddKnockbackForceMultiplier(KnockbackIncreaseAmount);
    }
    
    public override void Revert()
    {
        player.Stats.AddKnockbackForceMultiplier(-KnockbackIncreaseAmount);
    }
}
