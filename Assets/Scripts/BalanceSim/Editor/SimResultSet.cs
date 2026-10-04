using System;
using System.Collections.Generic;
using System.Linq;

namespace BalanceSim.Editor
{
    [Serializable]
    public class SimResultSet
    {
        public const string DateFormat = "yyyy-MM-dd HH:mm";

        public string createdAt;
        public string settingsName;
        public string sceneName;
        public string valuesName;
        public int runCount;
        public List<SimCaseValue> fixedValues = new();
        public List<SimResultCase> cases = new();
    }

    [Serializable]
    public class SimResultCase
    {
        public List<SimCaseValue> values = new();
        public SimResult result;

        public string Label => LabelOf(values);

        public static string LabelOf(IReadOnlyCollection<SimCaseValue> values)
        {
            return values.Count > 0 ? string.Join(", ", values) : "なし（基準）";
        }
    }

    [Serializable]
    public class SimCaseValue
    {
        public string target;
        public string property;
        public double value;

        public static List<SimCaseValue> Of(IEnumerable<SimValueRow> rows)
        {
            return rows.Select(r => new SimCaseValue { target = r.Value.TargetLabel, property = r.Value.PropertyLabel, value = r.Value.Value }).ToList();
        }

        public string Name => $"{target} {property}";

        public string ValueText => value.ToString("0.#####");

        public override string ToString() => $"{Name} = {ValueText}";
    }
}
