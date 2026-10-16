using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SpeedScaledPowerEffect", menuName = "Attributes/Effects/Speed Scaled Power")]
public class SpeedScaledPowerEffect : AttributeEffectDefinition
{
    [Tooltip("この水平速度（m/s）で効果が最大になる")]
    [SerializeField, Min(0.01f)] private float referenceSpeed = 15f;
    [Tooltip("最大のときに所属装備のダメージに足す割合（0.3 で +30%）")]
    [SerializeField, Min(0f)] private float maxDamageBonusRatio = 0.3f;
    [Tooltip("この水平速度（m/s）以上のときだけ、被ダメージを無効化できる")]
    [SerializeField, Min(0f)] private float negateSpeedThreshold = 15f;
    [Tooltip("被ダメージを無効化する確率")]
    [SerializeField, Range(0f, 1f)] private float negateChance = 0.3f;

    public override WeaponEffect CreateEffect(AttributeDefinition owner) => new Instance(this, owner);

    private float SpeedRatio(Player player) => player != null ? Mathf.Clamp01(player.HorizontalSpeed / referenceSpeed) : 0f;

    private sealed class Instance : WeaponEffect
    {
        private readonly SpeedScaledPowerEffect definition;
        private readonly AttributeDefinition owner;

        public Instance(SpeedScaledPowerEffect definition, AttributeDefinition owner)
        {
            this.definition = definition;
            this.owner = owner;
        }

        public override void ModifyStats(Weapon weapon, ref WeaponStatModifiers mods)
        {
            if (weapon.WeaponData.Attributes.Contains(owner))
                mods.DamageMultiplier *= 1f + definition.maxDamageBonusRatio * definition.SpeedRatio(weapon.WeaponOwner);
        }

        public override bool NegatesDamage(Player player) =>
            player != null && player.HorizontalSpeed >= definition.negateSpeedThreshold && Random.value < definition.negateChance;
    }
}
