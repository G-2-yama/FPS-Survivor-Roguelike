using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BalanceSim
{
    public static class Simulator
    {
        public static SimResult Run(SimInput input, Func<IUpgradeChoicePolicy> policyFactory)
        {
            var data = new SimData(input);
            var outputs = new SimRunOutput[input.runCount];
            Parallel.For(0, input.runCount, i =>
            {
                int seed = unchecked(input.seed * 1_000_003 + i);
                outputs[i] = new SimRun(data, policyFactory(), seed).Execute();
            });

            var result = new SimResult { sampleInterval = input.options.sampleInterval };
            List<SimSeries> defs = SimRun.SeriesDefs(input);
            for (int s = 0; s < defs.Count; s++)
            {
                result.series.Add(Aggregate(defs[s], outputs, s));
            }

            foreach (SimRunOutput output in outputs)
            {
                result.runs.Add(output.Summary);
            }

            result.message = Summarize(input, result);
            return result;
        }

        private static SimSeries Aggregate(SimSeries series, SimRunOutput[] outputs, int seriesIndex)
        {
            if (outputs.Length == 0)
            {
                return series;
            }

            int sampleCount = outputs[0].Samples[seriesIndex].Length;
            var values = new float[outputs.Length];
            for (int i = 0; i < sampleCount; i++)
            {
                for (int r = 0; r < outputs.Length; r++)
                {
                    values[r] = outputs[r].Samples[seriesIndex][i];
                }
                Array.Sort(values);
                series.mean.Add(values.Average());
                series.p10.Add(Percentile(values, 0.1f));
                series.p50.Add(Percentile(values, 0.5f));
                series.p90.Add(Percentile(values, 0.9f));
            }
            return series;
        }

        private static float Percentile(float[] sorted, float q)
        {
            return sorted[(int)MathF.Round(q * (sorted.Length - 1))];
        }

        private static string Summarize(SimInput input, SimResult result)
        {
            int runCount = result.runs.Count;
            if (runCount == 0)
            {
                return "実行回数が 0 です";
            }

            int cleared = result.runs.Count(r => r.cleared);
            var deaths = result.runs.Where(r => !r.cleared).ToList();
            string deathText = deaths.Count > 0 ? $"、死亡時刻の平均 {deaths.Average(r => r.endTime):F0}秒" : string.Empty;
            string text = $"{runCount}回: クリア {cleared}回 ({(float)cleared / runCount:P0}){deathText}、最終レベルの平均 {result.runs.Average(r => r.level):F1}、撃破数の平均 {result.runs.Average(r => r.kills):F0}";

            for (int g = 0; g < input.generators.Count; g++)
            {
                SimSeries alive = result.series[SimRun.FixedSeriesCount + g * 2];
                text += $"、{input.generators[g].name} の生存数の最大(平均) {alive.mean.DefaultIfEmpty(0f).Max():F0}";
            }
            return text;
        }
    }
}
