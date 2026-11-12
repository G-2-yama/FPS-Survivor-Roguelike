using UnityEngine;

public class Exp : PickupTriggerItem
{
    [SerializeField] private PickupEffectDefinition effectDefinition;
    public PickupEffectDefinition EffectDefinition => effectDefinition;
    public float ExpAmount => effectDefinition != null ? effectDefinition.GetAmount(PickupEffectKind.Experience) : 0f;
    protected override void OnPickup(Player player) => PickupEffectApplier.Apply(player, effectDefinition);
}