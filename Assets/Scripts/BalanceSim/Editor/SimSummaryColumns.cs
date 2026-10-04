using System;
using System.Collections.Generic;
using System.Linq;

namespace BalanceSim.Editor
{
    public class SimSummaryColumns
    {
        public const string Missing = "-";
        private static readonly string[] MetricHeaders = { "クリア率", "死亡時刻の平均", "最終レベルの平均", "撃破数の平均" };
        private static readonly Func<double, string>[] MetricFormats =
        {
            x => $"{x:0}%",
            Seconds,
            x => $"{x:0.0}",
            x => $"{x:0}",
        };

        public readonly string[] Headers;
        public readonly Func<double, string>[] Formats;
        public readonly double?[][] Keys;
        public readonly int ValueCount;

        private SimSummaryColumns(string[] headers, Func<double, string>[] formats, double?[][] keys, int valueCount)
        {
            Headers = headers;
            Formats = formats;
            Keys = keys;
            ValueCount = valueCount;
        }

        public int Count => Headers.Length;

        public int CaseCount => Keys.Length;

        public int FirstValue => 1;

        public int FirstMetric => 1 + ValueCount;

        public string Format(int column, double? key) => key.HasValue ? Formats[column](key.Value) : Missing;

        public bool IsBaseline(int caseIndex)
        {
            for (int col = FirstValue; col < FirstMetric; col++)
            {
                if (Keys[caseIndex][col].HasValue)
                {
                    return false;
                }
            }
            return true;
        }

        public static SimSummaryColumns Of(SimResultSet set)
        {
            List<string> weapons = set.cases
                .SelectMany(c => c.result.evolutionStats)
                .Where(s => s.count > 0)
                .Select(s => s.weapon)
                .Distinct()
                .ToList();
            List<Dictionary<string, SimCaseValue>> keyedValues = set.cases.Select(c => KeyedValues(c.values)).ToList();
            var valueColumns = new List<(string key, string name)>();
            var columnKeys = new HashSet<string>();
            foreach (Dictionary<string, SimCaseValue> values in keyedValues)
            {
                foreach (KeyValuePair<string, SimCaseValue> entry in values)
                {
                    if (columnKeys.Add(entry.Key))
                    {
                        valueColumns.Add((entry.Key, entry.Value.Name));
                    }
                }
            }

            string[] headers = new[] { "#" }
                .Concat(valueColumns.Select(column => column.name))
                .Concat(MetricHeaders)
                .Concat(weapons.SelectMany(w => new[] { $"{w} 進化率", $"{w} 進化時刻の平均" }))
                .ToArray();
            Func<double, string>[] formats = new Func<double, string>[] { x => $"{x:0}" }
                .Concat(valueColumns.Select(_ => (Func<double, string>)(x => x.ToString("0.#####"))))
                .Concat(MetricFormats)
                .Concat(weapons.SelectMany(_ => new Func<double, string>[] { x => $"{x:0}%", Seconds }))
                .ToArray();
            double?[][] keys = set.cases
                .Select((c, i) => new double?[] { i + 1 }
                    .Concat(valueColumns.Select(column => keyedValues[i].TryGetValue(column.key, out SimCaseValue v) ? v.value : (double?)null))
                    .Concat(MetricKeys(c.result))
                    .Concat(weapons.SelectMany(w => EvolutionKeys(c.result, w)))
                    .ToArray())
                .ToArray();

            return new SimSummaryColumns(headers, formats, keys, valueColumns.Count);
        }

        private static Dictionary<string, SimCaseValue> KeyedValues(List<SimCaseValue> values)
        {
            var keyed = new Dictionary<string, SimCaseValue>();
            var occurrences = new Dictionary<string, int>();
            foreach (SimCaseValue value in values)
            {
                occurrences.TryGetValue(value.Name, out int count);
                occurrences[value.Name] = count + 1;
                keyed.Add($"{value.Name}\n{count}", value);
            }
            return keyed;
        }

        private static double?[] MetricKeys(SimResult result)
        {
            if (result.runCount == 0)
            {
                return MetricHeaders.Select(_ => (double?)null).ToArray();
            }

            return new double?[]
            {
                100.0 * result.clearedCount / result.runCount,
                result.clearedCount < result.runCount ? result.deathTimeMean : null,
                result.levelMean,
                result.killsMean,
            };
        }

        private static double?[] EvolutionKeys(SimResult result, string weapon)
        {
            SimEvolutionStat stat = result.evolutionStats.FirstOrDefault(s => s.weapon == weapon);
            if (stat == null || result.runCount == 0)
            {
                return new double?[] { null, null };
            }

            return new double?[]
            {
                100.0 * stat.count / result.runCount,
                stat.count > 0 ? stat.timeMean : null,
            };
        }

        private static string Seconds(double x) => $"{x:0}秒";
    }
}
