using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "KnockbackBonusEffect", menuName = "Attributes/Effects/Knockback Bonus")]
public class KnockbackBonusEffect : AttributeEffectDefinition
{
    [Tooltip("所属装備の吹き飛ばしに足す値")]
    [SerializeField, Min(0f)] private float knockbackBonus = 20f;

    public override WeaponEffect CreateEffect(AttributeDefinition owner) => new Instance(this, owner);

    private sealed class Instance : WeaponEffect
    {
        private readonly KnockbackBonusEffect definition;
        private readonly AttributeDefinition owner;

        public Instance(KnockbackBonusEffect definition, AttributeDefinition owner)
        {
            this.definition = definition;
            this.owner = owner;
        }

        public override void ModifyStats(Weapon weapon, ref WeaponStatModifiers mods)
        {
            if (weapon.WeaponData.Attributes.Contains(owner))
                mods.KnockbackBonus += definition.knockbackBonus;
        }
    }
}
