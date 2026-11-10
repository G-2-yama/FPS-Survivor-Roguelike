using UnityEngine;

public class healingHpitem : PickupTriggerItem
{
    [SerializeField] private PickupEffectDefinition effectDefinition;
    public PickupEffectDefinition EffectDefinition => effectDefinition;
    public float HealAmount => effectDefinition != null ? effectDefinition.GetAmount(PickupEffectKind.Heal) : 0f;
    protected override void OnPickup(Player player)
    {
        if (PickupEffectApplier.Apply(player, effectDefinition)) player.Sounder.Play(SoundCategory.GetItem, 6);
    }
}