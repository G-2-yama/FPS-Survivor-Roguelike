using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ShotStackDamageEffect", menuName = "Attributes/Effects/Shot Stack Damage")]
public class ShotStackDamageEffect : AttributeEffectDefinition
{
    [Tooltip("リロード後にこの発数を撃つごとにダメージが増える")]
    [SerializeField, Min(1)] private int shotsPerStep = 5;
    [Tooltip("1段ごとに足すダメージ")]
    [SerializeField, Min(0)] private int damagePerStep = 1;

    public override WeaponEffect CreateEffect(AttributeDefinition owner) => new Instance(this, owner);

    private sealed class Instance : WeaponEffect
    {
        private readonly ShotStackDamageEffect definition;
        private readonly AttributeDefinition owner;

        public Instance(ShotStackDamageEffect definition, AttributeDefinition owner)
        {
            this.definition = definition;
            this.owner = owner;
        }

        public override void ModifyStats(Weapon weapon, ref WeaponStatModifiers mods)
        {
            if (weapon.WeaponData.Attributes.Contains(owner))
                mods.FlatDamageBonus += weapon.Runtime.ShotsSinceReload / definition.shotsPerStep * definition.damagePerStep;
        }
    }
}
