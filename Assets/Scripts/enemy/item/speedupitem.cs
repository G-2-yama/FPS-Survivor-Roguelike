using UnityEngine;

public class speedupitem : PickupTriggerItem
{
    [SerializeField] private PickupEffectDefinition effectDefinition;
    public PickupEffectDefinition EffectDefinition => effectDefinition;
    public float SpeedAmount => effectDefinition != null ? effectDefinition.GetAmount(PickupEffectKind.RunSpeed) : 0f;
    public float Duration => effectDefinition != null ? effectDefinition.Duration : 0f;
    protected override void OnPickup(Player player)
    {
        if (PickupEffectApplier.Apply(player, effectDefinition)) player.Sounder.Play(SoundCategory.GetItem, 7);
    }
}