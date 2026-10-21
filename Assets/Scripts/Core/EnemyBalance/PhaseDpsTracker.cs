using System;

namespace EnemyBalance
{
    /// <summary>通常Phaseの実ダメージを集計し、次のExPhaseで固定するHP倍率を求める。</summary>
    public sealed class PhaseDpsTracker
    {
        private bool initialized;
        private bool isExPhase;
        private double normalStartTime;
        private long damage;

        public double PreviousNormalDps { get; private set; }
        public bool HasPreviousNormalDps { get; private set; }

        public void BeginPhase(bool nextIsExPhase, double startTime)
        {
            if (initialized && !isExPhase)
            {
                double duration = startTime - normalStartTime;
                HasPreviousNormalDps = duration > 0 && damage > 0;
                PreviousNormalDps = HasPreviousNormalDps ? damage / duration : 0;
            }

            isExPhase = nextIsExPhase;
            initialized = true;
            if (!nextIsExPhase)
            {
                normalStartTime = startTime;
                damage = 0;
            }
        }

        public void RecordDamage(int actualDamage)
        {
            if (initialized && !isExPhase && actualDamage > 0)
                damage += actualDamage;
        }

        public double GetHpMultiplier(double referenceDps, double minimum, double maximum)
        {
            // 実測DPSは上乗せ補正にだけ使い、低DPSでも基礎HPを下げない。
            minimum = Math.Max(1, minimum);
            maximum = Math.Max(minimum, maximum);
            double multiplier = HasPreviousNormalDps && referenceDps > 0
                ? Math.Sqrt(PreviousNormalDps / referenceDps)
                : 1;
            return Math.Max(minimum, Math.Min(maximum, multiplier));
        }

        public static int ScaleHp(int baseHp, double multiplier)
        {
            double hp = Math.Round(baseHp * multiplier, MidpointRounding.AwayFromZero);
            return (int)Math.Max(1, Math.Min(int.MaxValue, hp));
        }
    }
}
