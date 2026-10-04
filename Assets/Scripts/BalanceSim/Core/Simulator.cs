using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BalanceSim
{
    public sealed class SimBatchCase
    {
        public SimInput Input;
        public Func<IUpgradeChoicePolicy> PolicyFactory;
    }

    public static class Simulator
    {
        public static SimResult Run(SimInput input, Func<IUpgradeChoicePolicy> policyFactory)
        {
            var data = new SimData(input);
            var outputs = new SimRunOutput[input.runCount];
            Parallel.For(0, input.runCount, i => outputs[i] = RunOne(data, policyFactory, i));
            return Collect(input, outputs);
        }

        public static void RunBatch(int caseCount, Func<int, SimBatchCase> load, Action<int, SimResult> onCompleted)
        {
            var gate = new object();
            int nextCase = 0;
            BatchCase current = null;
            bool failed = false;

            Parallel.For(0, Environment.ProcessorCount, _ =>
            {
                try
                {
                    while (true)
                    {
                        BatchCase target;
                        int run;
                        lock (gate)
                        {
                            if (failed)
                            {
                                return;
                            }
                            if (current == null || current.NextRun >= current.Outputs.Length)
                            {
                                if (nextCase >= caseCount)
                                {
                                    return;
                                }
                                current = BatchCase.Open(nextCase, load(nextCase));
                                nextCase++;
                            }
                            target = current;
                            run = target.NextRun++;
                        }

                        SimRunOutput output;
                        try
                        {
                            output = RunOne(target.Data, target.PolicyFactory, run);
                        }
                        catch (Exception e)
                        {
                            throw new InvalidOperationException($"{target.Index + 1} 通り目の {run + 1} 回目で失敗しました", e);
                        }

                        bool completed;
                        lock (gate)
                        {
                            target.Outputs[run] = output;
                            completed = --target.Remaining == 0;
                        }
                        if (completed)
                        {
                            onCompleted(target.Index, Collect(target.Data.Input, target.Outputs));
                        }
                    }
                }
                catch
                {
                    lock (gate)
                    {
                        failed = true;
                    }
                    throw;
                }
            });
        }

        private static SimRunOutput RunOne(SimData data, Func<IUpgradeChoicePolicy> policyFactory, int index)
        {
            int seed = unchecked(data.Input.seed * 1_000_003 + index);
            return new SimRun(data, policyFactory(), seed).Execute();
        }

        private static SimResult Collect(SimInput input, SimRunOutput[] outputs)
        {
            bool keepDetails = input.options.keepRunDetails;
            var result = new SimResult { sampleInterval = input.options.sampleInterval };
            List<SimSeries> defs = SimRun.SeriesDefs(input);
            for (int s = 0; s < defs.Count; s++)
            {
                result.series.Add(Aggregate(defs[s], outputs, s, keepDetails));
            }

            List<SimRunSummary> runs = outputs.Select(o => o.Summary).ToList();
            AddStats(input, result, runs);
            if (keepDetails)
            {
                result.runs.AddRange(runs);
            }

            result.message = Summarize(input, result);
            return result;
        }

        private static void AddStats(SimInput input, SimResult result, List<SimRunSummary> runs)
        {
            result.runCount = runs.Count;
            if (runs.Count == 0)
            {
                return;
            }

            result.clearedCount = runs.Count(r => r.cleared);
            List<SimRunSummary> deaths = runs.Where(r => !r.cleared).ToList();
            result.deathTimeMean = deaths.Count > 0 ? deaths.Average(r => r.endTime) : 0f;
            result.levelMean = runs.Average(r => r.level);
            result.killsMean = runs.Average(r => r.kills);

            foreach (string weapon in Progression.EvolvedWeapons(input))
            {
                List<float> times = runs
                    .Select(r => r.evolutions.FirstOrDefault(e => e.weapon == weapon))
                    .Where(e => e != null)
                    .Select(e => e.time)
                    .ToList();
                result.evolutionStats.Add(new SimEvolutionStat
                {
                    weapon = weapon,
                    count = times.Count,
                    timeMean = times.Count > 0 ? times.Average() : 0f,
                });
            }
        }

        private static SimSeries Aggregate(SimSeries series, SimRunOutput[] outputs, int seriesIndex, bool withPercentiles)
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
                if (withPercentiles)
                {
                    series.p10.Add(Percentile(values, 0.1f));
                    series.p50.Add(Percentile(values, 0.5f));
                    series.p90.Add(Percentile(values, 0.9f));
                }
            }
            return series;
        }

        private static float Percentile(float[] sorted, float q)
        {
            return sorted[(int)MathF.Round(q * (sorted.Length - 1))];
        }

        private static string Summarize(SimInput input, SimResult result)
        {
            int runCount = result.runCount;
            if (runCount == 0)
            {
                return "実行回数が 0 です";
            }

            int cleared = result.clearedCount;
            string deathText = cleared < runCount ? $"、死亡時刻の平均 {result.deathTimeMean:F0}秒" : string.Empty;
            string text = $"{runCount}回: クリア {cleared}回 ({(float)cleared / runCount:P0}){deathText}、最終レベルの平均 {result.levelMean:F1}、撃破数の平均 {result.killsMean:F0}";

            for (int g = 0; g < input.generators.Count; g++)
            {
                SimSeries alive = result.series[SimRun.FixedSeriesCount + g * 2];
                text += $"、{input.generators[g].name} の生存数の最大(平均) {alive.mean.DefaultIfEmpty(0f).Max():F0}";
            }
            return text;
        }

        private sealed class BatchCase
        {
            public int Index;
            public SimData Data;
            public Func<IUpgradeChoicePolicy> PolicyFactory;
            public SimRunOutput[] Outputs;
            public int NextRun;
            public int Remaining;

            public static BatchCase Open(int index, SimBatchCase source)
            {
                if (source.Input.runCount <= 0)
                {
                    throw new InvalidOperationException($"{index + 1} 通り目: 実行回数が 0 以下です");
                }

                return new BatchCase
                {
                    Index = index,
                    Data = new SimData(source.Input),
                    PolicyFactory = source.PolicyFactory,
                    Outputs = new SimRunOutput[source.Input.runCount],
                    Remaining = source.Input.runCount,
                };
            }
        }
    }
}
