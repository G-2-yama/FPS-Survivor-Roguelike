using System;
using System.Collections.Generic;
using System.Linq;

namespace EnemyBalance
{
    /// <summary>最低数の補充・追加生成・Phase切替から生存敵数の期待値を計算する。</summary>
    public static class EnemySpawnPreview
    {
        public sealed class Phase
        {
            public double Start;
            public int Minimum;
            public double Interval;
        }

        public readonly struct Point
        {
            public readonly double Time;
            public readonly double Alive;
            public readonly int PhaseIndex;
            public Point(double time, double alive, int phaseIndex)
            { Time = time; Alive = alive; PhaseIndex = phaseIndex; }
        }

        public sealed class Result
        {
            public readonly List<Point> Points = new List<Point>();
            public double Spawned;
            public double Replaced;
            public double Peak;

            /// <summary>生成の瞬間は生成後の値を返し、間の撃破による減少は線形補間する。</summary>
            public Point At(double time)
            {
                if (Points.Count == 0) return new Point(time, 0, 0);
                int lo = 0, hi = Points.Count;
                while (lo < hi)
                {
                    int mid = lo + (hi - lo) / 2;
                    if (Points[mid].Time <= time) lo = mid + 1;
                    else hi = mid;
                }
                Point before = Points[Math.Max(0, lo - 1)];
                if (lo == Points.Count || time <= before.Time) return before;
                Point after = Points[lo];
                double t = (time - before.Time) / (after.Time - before.Time);
                return new Point(time, before.Alive + (after.Alive - before.Alive) * t, before.PhaseIndex);
            }
        }

        public static Result Run(IEnumerable<Phase> source, double duration, int capacity, double killsPerSecond)
        {
            if (!Finite(duration) || duration <= 0 || capacity <= 0
                || !Finite(killsPerSecond) || killsPerSecond < 0)
                throw new ArgumentException("表示時間・生成上限は正、撃破数/秒は0以上にしてください。");
            if (source == null) throw new ArgumentNullException(nameof(source));
            Phase[] phases = source.ToArray();
            if (phases.Length == 0) return new Result();
            if (phases.Any(p => p == null)) throw new ArgumentException("Phaseに空の参照があります。");
            phases = phases.OrderBy(p => p.Start).ToArray();
            for (int i = 0; i < phases.Length; i++)
            {
                Phase p = phases[i];
                if (!Finite(p.Start) || p.Start < 0 || !Finite(p.Interval) || p.Interval < 0.001
                    || p.Minimum < 0 || (i > 0 && p.Start == phases[i - 1].Start))
                    throw new ArgumentException("開始時刻は重複しない0以上、最低出現数は0以上、出現間隔は0.001秒以上にしてください。");
            }

            var result = new Result();
            int index = 0;
            // 実Generatorと同様に先頭Phaseは0秒から有効。最終Phaseは終端で停止しない。
            double now = 0, alive = 0, nextSpawn = 0, nextSample = 0;
            double step = Math.Max(0.1, duration / 1800);
            for (int events = 0; ; events++)
            {
                if (events > 250000)
                    throw new ArgumentException("計算量が多すぎます。表示時間を短くするか出現頻度を下げてください。");
                double boundary = index + 1 < phases.Length ? phases[index + 1].Start : double.PositiveInfinity;
                double time = Math.Min(duration, Math.Min(nextSample, Math.Min(nextSpawn, boundary)));
                alive = Math.Max(0, alive - killsPerSecond * (time - now));
                now = time;
                bool changed = boundary <= now;
                bool spawn = nextSpawn <= now || changed;
                if (changed) index++;
                if (spawn)
                {
                    result.Points.Add(new Point(now, alive, index));
                    double count = alive < phases[index].Minimum ? phases[index].Minimum - alive : 1;
                    result.Spawned += count;
                    result.Replaced += Math.Max(0, alive + count - capacity);
                    alive = Math.Min(capacity, alive + count);
                    result.Peak = Math.Max(result.Peak, alive);
                    result.Points.Add(new Point(now, alive, index));
                    nextSpawn = now + phases[index].Interval;
                }
                if (nextSample <= now)
                {
                    result.Points.Add(new Point(now, alive, index));
                    nextSample += step;
                }
                if (now >= duration)
                {
                    result.Points.Add(new Point(duration, alive, index));
                    return result;
                }
            }
        }

        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
