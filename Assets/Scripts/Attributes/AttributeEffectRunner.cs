using System.Collections.Generic;

public class AttributeEffectRunner
{
    private readonly Player player;
    private readonly AttributeTracker tracker;
    private readonly Dictionary<AttributeDefinition, List<WeaponEffect>> appliedActive = new Dictionary<AttributeDefinition, List<WeaponEffect>>();
    private readonly Dictionary<AttributeDefinition, List<WeaponEffect>> appliedMax = new Dictionary<AttributeDefinition, List<WeaponEffect>>();
    private readonly Dictionary<AttributeDefinition, List<WeaponEffect>> appliedCondition = new Dictionary<AttributeDefinition, List<WeaponEffect>>();

    public AttributeEffectRunner(Player player, AttributeTracker tracker)
    {
        this.player = player;
        this.tracker = tracker;
        tracker.Changed += Refresh;
    }

    public void Dispose()
    {
        tracker.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (tracker.Catalog == null)
            return;

        foreach (AttributeDefinition attribute in tracker.Catalog.All)
        {
            if (attribute == null)
                continue;

            Apply(attribute, tracker.IsActive(attribute), attribute.ActiveEffects, appliedActive);
            Apply(attribute, tracker.IsMax(attribute), attribute.MaxEffects, appliedMax);
            Apply(attribute, tracker.IsConditionMet(attribute), attribute.ConditionEffects, appliedCondition);
        }
    }

    private void Apply(AttributeDefinition attribute, bool shouldApply,
        IReadOnlyList<AttributeEffectDefinition> definitions, Dictionary<AttributeDefinition, List<WeaponEffect>> applied)
    {
        bool isApplied = applied.TryGetValue(attribute, out List<WeaponEffect> effects);
        if (shouldApply == isApplied)
            return;

        if (shouldApply)
        {
            effects = new List<WeaponEffect>();
            foreach (AttributeEffectDefinition definition in definitions)
            {
                WeaponEffect effect = definition != null ? definition.CreateEffect(attribute) : null;
                if (effect == null)
                    continue;

                effects.Add(effect);
                player.AddWeaponEffect(effect);
            }
            applied[attribute] = effects;
        }
        else
        {
            foreach (WeaponEffect effect in effects)
            {
                player.RemoveWeaponEffect(effect);
            }
            applied.Remove(attribute);
        }
    }
}
