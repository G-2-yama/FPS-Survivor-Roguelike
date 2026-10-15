using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ShotStackDamageEffect", menuName = "Attributes/Effects/Shot Stack Damage")]
public class ShotStackDamageEffect : AttributeEffectDefinition
{
    [Tooltip("リロード後に1発撃つごとに増えるダメージの割合（0.001 で +0.1%）")]
    [SerializeField, Min(0f)] private float damageBonusPerShot = 0.001f;

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
                mods.DamageMultiplier *= 1f + definition.damageBonusPerShot * weapon.Runtime.ShotsSinceReload;
        }
    }
}
