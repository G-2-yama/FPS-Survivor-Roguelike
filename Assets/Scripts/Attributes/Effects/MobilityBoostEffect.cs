using UnityEngine;

[CreateAssetMenu(fileName = "MobilityBoostEffect", menuName = "Attributes/Effects/Mobility Boost")]
public class MobilityBoostEffect : AttributeEffectDefinition
{
    [Tooltip("歩き・走りの速度に足す割合（0.15 で +15%）")]
    [SerializeField, Min(0f)] private float moveSpeedBonusRatio = 0.15f;
    [Tooltip("ジャンプの初速に足す割合（0.1 で +10%）")]
    [SerializeField, Min(0f)] private float jumpForceBonusRatio = 0.1f;
    [Tooltip("ダッシュの速度の倍率。距離は変えず、時間を短くする")]
    [SerializeField, Min(1f)] private float dashSpeedMultiplier = 1.3f;
    [Tooltip("ダッシュのクールタイムから引く秒数")]
    [SerializeField, Min(0f)] private float dashCooldownReduction;
    [Tooltip("ダッシュの距離に足す割合（0.5 で +50%）。時間は変えないので、速度も同じ割合で上がる")]
    [SerializeField, Min(0f)] private float dashDistanceBonusRatio;
    [Tooltip("ダッシュ中の被ダメージを無効化する")]
    [SerializeField] private bool dashInvincible;

    public override WeaponEffect CreateEffect(AttributeDefinition owner) => new Instance(this);

    private sealed class Instance : WeaponEffect
    {
        private readonly MobilityBoostEffect definition;
        private float walkSpeedDelta;
        private float runSpeedDelta;
        private float jumpForceDelta;
        private float dashDurationDelta;
        private float dashCooldownDelta;
        private float dashDistanceDelta;

        public Instance(MobilityBoostEffect definition)
        {
            this.definition = definition;
        }

        public override void OnAdded(Player player)
        {
            PlayerStats stats = player.Stats;
            walkSpeedDelta = stats.WalkSpeed * definition.moveSpeedBonusRatio;
            runSpeedDelta = stats.RunSpeed * definition.moveSpeedBonusRatio;
            jumpForceDelta = stats.JumpForce * definition.jumpForceBonusRatio;
            dashDurationDelta = stats.DashDuration * (1f / definition.dashSpeedMultiplier - 1f);
            dashCooldownDelta = -Mathf.Min(definition.dashCooldownReduction, stats.DashCooldown);
            dashDistanceDelta = stats.DashDistance * definition.dashDistanceBonusRatio;

            stats.AddWalkSpeed(walkSpeedDelta);
            stats.AddRunSpeed(runSpeedDelta);
            stats.AddJumpForce(jumpForceDelta);
            stats.AddDashDuration(dashDurationDelta);
            stats.AddDashCooldown(dashCooldownDelta);
            stats.AddDashDistance(dashDistanceDelta);
        }

        public override void OnRemoved(Player player)
        {
            PlayerStats stats = player.Stats;
            stats.AddWalkSpeed(-walkSpeedDelta);
            stats.AddRunSpeed(-runSpeedDelta);
            stats.AddJumpForce(-jumpForceDelta);
            stats.AddDashDuration(-dashDurationDelta);
            stats.AddDashCooldown(-dashCooldownDelta);
            stats.AddDashDistance(-dashDistanceDelta);
        }

        public override bool NegatesDamage(Player player) => definition.dashInvincible && player.IsDashing;
    }
}
