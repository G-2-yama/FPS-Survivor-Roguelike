using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MagazineGrowthEffect", menuName = "Attributes/Effects/Magazine Growth")]
public class MagazineGrowthEffect : AttributeEffectDefinition
{
    [Tooltip("リロード開始時の弾の消費率がこの値以上なら弾倉が成長する")]
    [SerializeField, Range(0f, 1f)] private float consumedRatioThreshold = 0.7f;
    [Tooltip("成長1回で弾倉に掛ける倍率")]
    [SerializeField, Min(1f)] private float growthMultiplier = 1.1f;
    [Tooltip("撃ち切ってからリロードしたときに弾倉に掛ける倍率")]
    [SerializeField, Min(1f)] private float fullConsumedGrowthMultiplier = 1.2f;

    public override WeaponEffect CreateEffect(AttributeDefinition owner) => new Instance(this, owner);

    private sealed class Instance : WeaponEffect
    {
        private readonly MagazineGrowthEffect definition;
        private readonly AttributeDefinition owner;

        public Instance(MagazineGrowthEffect definition, AttributeDefinition owner)
        {
            this.definition = definition;
            this.owner = owner;
        }

        public override void ModifyStats(Weapon weapon, ref WeaponStatModifiers mods)
        {
            if (Belongs(weapon))
                mods.MagazineMultiplier *= weapon.Runtime.MagazineScale;
        }

        public override void OnReloadComplete(Weapon weapon)
        {
            if (!Belongs(weapon))
                return;

            float consumedRatio = weapon.Runtime.LastReloadConsumedRatio;
            if (consumedRatio >= 1f)
                weapon.Runtime.MagazineScale *= definition.fullConsumedGrowthMultiplier;
            else if (consumedRatio >= definition.consumedRatioThreshold)
                weapon.Runtime.MagazineScale *= definition.growthMultiplier;
        }

        private bool Belongs(Weapon weapon) => weapon.WeaponData.Attributes.Contains(owner);
    }
}
