using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AreaExtensionEffect", menuName = "Attributes/Effects/Area Extension")]
public class AreaExtensionEffect : AttributeEffectDefinition
{
    [Header("Activate")]
    [Tooltip("所属武器のエリアの持続時間に足す秒数（レベルに関係ない分）")]
    [SerializeField, Min(0f)] private float baseExtraLifetime = 3f;
    [Tooltip("レベル1つごとに足す秒数")]
    [SerializeField, Min(0f)] private float extraLifetimePerLevel = 1f;

    [Header("Condition")]
    [Tooltip("所持中の所属武器のエリアの持続時間の合計がこの秒数以上で条件を満たす")]
    [SerializeField, Min(0f)] private float requiredTotalLifetime = 30f;
    [Tooltip("条件を満たしたとき、エリアの生成から1秒ごとに足すダメージ")]
    [SerializeField, Min(0)] private int damagePerSecond = 1;

    public override WeaponEffect CreateEffect(AttributeDefinition owner) => new Instance(this, owner);

    private sealed class Instance : WeaponEffect
    {
        private readonly AreaExtensionEffect definition;
        private readonly AttributeDefinition owner;

        public Instance(AreaExtensionEffect definition, AttributeDefinition owner)
        {
            this.definition = definition;
            this.owner = owner;
        }

        public override void ModifyStats(Weapon weapon, ref WeaponStatModifiers mods)
        {
            if (!Belongs(weapon))
                return;

            mods.AreaExtraLifetime += ExtraLifetime(weapon);
            if (IsConditionMet(weapon.WeaponOwner))
                mods.AreaDamagePerSecond += definition.damagePerSecond;
        }

        private float ExtraLifetime(Weapon weapon) =>
            definition.baseExtraLifetime + definition.extraLifetimePerLevel * (weapon.Level + 1);

        private bool IsConditionMet(Player player)
        {
            if (player == null || player.Inventory.WeaponSlots == null)
                return false;

            float total = 0f;
            foreach (Weapon weapon in player.Inventory.WeaponSlots.Values)
            {
                if (weapon == null || !Belongs(weapon))
                    continue;

                float lifetime = weapon.FireModeData != null ? weapon.FireModeData.AreaLifetime : 0f;
                if (lifetime > 0f)
                    total += lifetime + ExtraLifetime(weapon);
            }
            return total >= definition.requiredTotalLifetime;
        }

        private bool Belongs(Weapon weapon) => weapon.HasWeapon && weapon.WeaponData.Attributes.Contains(owner);
    }
}
