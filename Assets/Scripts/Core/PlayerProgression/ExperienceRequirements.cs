using System;

namespace BalanceSim
{
    /// <summary>本体・Inspector・シミュレータで共用するレベル別必要経験値の計算</summary>
    public static class ExperienceRequirements
    {
        public static double At(int initialLevel, float first, float rate, float[] levels, int level)
        {
            if (levels != null && levels.Length > 0)
                return levels[(int)Math.Min(levels.Length - 1L, Math.Max(0L, (long)level - initialLevel))];
            return first * Math.Pow(rate, (double)level - initialLevel);
        }

        // 到達レベル自身の次回必要EXPは含めない。テーブル末尾以降は末尾の値で一定。
        public static double Total(int initialLevel, float first, float rate, float[] levels, int start, int target)
        {
            if (target <= start) return 0d;
            if (levels == null || levels.Length == 0)
            {
                double requirement = At(initialLevel, first, rate, levels, start);
                return rate == 1f ? requirement * (target - start)
                    : requirement * ((Math.Pow(rate, target - start) - 1d) / (rate - 1d));
            }
            double total = 0d;
            long level = start;
            for (; level < target && level - initialLevel < levels.Length; level++)
                total += At(initialLevel, first, rate, levels, (int)level);
            return total + (double)(target - level) * levels[levels.Length - 1];
        }
    }

}
