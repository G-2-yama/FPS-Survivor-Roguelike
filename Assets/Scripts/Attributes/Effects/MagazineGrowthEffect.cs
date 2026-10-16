using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MagazineGrowthEffect", menuName = "Attributes/Effects/Magazine Growth")]
public class MagazineGrowthEffect : AttributeEffectDefinition
{
    [Tooltip("リロード開始時の弾の消費率がこの値以上なら弾倉が成長する")]
    [SerializeField, Range(0f, 1f)] private float consumedRatioThreshold = 0.7f;
    [Tooltip("成長1回で弾倉に掛ける倍率")]
    [SerializeField, Min(1f)] private float growthMultiplier = 1.05f;
    [Tooltip("撃ち切ってからリロードしたときに弾倉に掛ける倍率")]
    [SerializeField, Min(1f)] private float fullConsumedGrowthMultiplier = 1.1f;
    [Tooltip("弾倉の倍率の上限（5 で元の 500%）")]
    [SerializeField, Min(1f)] private float maxScale = 5f;
    [Tooltip("上限を掛けない武器")]
    [SerializeField] private List<WeaponData> uncappedWeapons = new List<WeaponData>();
    [Tooltip("所属していても弾倉が成長しない武器")]
    [SerializeField] private List<WeaponData> excludedWeapons = new List<WeaponData>();

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
            if (Grows(weapon))
                mods.MagazineMultiplier *= weapon.Runtime.MagazineScale;
        }

        public override void OnReloadComplete(Weapon weapon)
        {
            if (!Grows(weapon))
                return;

            float consumedRatio = weapon.Runtime.LastReloadConsumedRatio;
            float multiplier;
            if (consumedRatio >= 1f)
                multiplier = definition.fullConsumedGrowthMultiplier;
            else if (consumedRatio >= definition.consumedRatioThreshold)
                multiplier = definition.growthMultiplier;
            else
                return;

            float scale = weapon.Runtime.MagazineScale * multiplier;
            if (!definition.uncappedWeapons.Contains(weapon.WeaponData))
                scale = Mathf.Min(scale, definition.maxScale);
            weapon.Runtime.MagazineScale = scale;
        }

        private bool Grows(Weapon weapon) =>
            weapon.WeaponData.Attributes.Contains(owner) && !definition.excludedWeapons.Contains(weapon.WeaponData);
    }
}
