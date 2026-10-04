using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BalanceSim.Editor
{
    [CreateAssetMenu(fileName = "BalanceSimSweep", menuName = "BalanceSim/Sweep")]
    public class SimSweep : SimValueSource
    {
        public const string GeneratorField = nameof(generator);

        [Tooltip("振らずに固定しておく値（数値SO）。空なら、振る値以外はゲームの値のまま")]
        [SerializeField] private SimValueSet fixedValues;
        public SimValueSet FixedValues => fixedValues;

        [Tooltip("振る値。行ごとに、最小から最大まで刻みずつの値を候補にする")]
        [SerializeField] private List<SimSweepAxis> axes = new();

        [Tooltip("まとめて倍率で振る値。1行に入れた全部の対象に同じ倍率を掛け、1つの振る値として扱う")]
        [SerializeField] private List<SimSweepGroup> groups = new();

        [Tooltip("振る値の組み合わせの作り方。ISimSweepGenerator を実装したクラスを足すと一覧に出る")]
        [SerializeReference] private ISimSweepGenerator generator = new GridSweep();
        public ISimSweepGenerator Generator => generator;

        public int CountCases()
        {
            List<int> counts = axes.Select((axis, i) => axis.Values(AxisRowName(i)).Count)
                .Concat(groups.Select((group, i) => group.Factors(GroupRowName(i)).Count))
                .ToList();
            return WithName(() => RequireGenerator().CountCases(RequireDimensions(counts)));
        }

        public override List<SimValueCase> BuildCases()
        {
            List<List<SimSweepChoice>> dimensions = Dimensions();
            List<int> counts = RequireDimensions(dimensions.Select(d => d.Count).ToList());
            return ToCases(dimensions, WithName(() => RequireGenerator().Generate(counts)));
        }

        public override List<SimValueCase> BuildNextCases(IReadOnlyList<SimValueCase> done, IReadOnlyList<SimResult> results,
            List<string> warnings, out string conclusion)
        {
            conclusion = null;
            if (generator is not ISimAdaptiveSweepGenerator adaptive)
            {
                return new List<SimValueCase>();
            }

            List<List<SimSweepChoice>> dimensions = Dimensions();
            List<int> counts = RequireDimensions(dimensions.Select(d => d.Count).ToList());
            List<SimSweepOutcome> outcomes = done.Select((c, i) => new SimSweepOutcome(c.Pick, SimResultCase.LabelOf(c.Labels), results[i])).ToList();
            string stop = null;
            List<int[]> picks = WithName(() => adaptive.Next(counts, outcomes, warnings, out stop));
            conclusion = stop;
            return ToCases(dimensions, picks);
        }

        private List<List<SimSweepChoice>> Dimensions()
        {
            return axes.Select((axis, i) => axis.Choices(AxisRowName(i)))
                .Concat(groups.Select((group, i) => group.Choices(GroupRowName(i))))
                .ToList();
        }

        private List<SimValueCase> ToCases(List<List<SimSweepChoice>> dimensions, List<int[]> picks)
        {
            List<SimValueRow> fixedRows = fixedValues != null ? fixedValues.Rows() : new List<SimValueRow>();
            return picks.Select(pick =>
            {
                List<SimSweepChoice> chosen = pick.Select((choice, d) => choice >= 0 ? dimensions[d][choice] : null)
                    .Where(c => c != null)
                    .ToList();
                return new SimValueCase(fixedRows, chosen.SelectMany(c => c.Rows).ToList(), chosen.Select(c => c.Label).ToList(), pick);
            }).ToList();
        }

        private List<int> RequireDimensions(List<int> counts)
        {
            if (counts.Count == 0)
            {
                throw new InvalidOperationException($"{name}: 振る値の行がありません");
            }
            return counts;
        }

        private T WithName<T>(Func<T> generate)
        {
            try
            {
                return generate();
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException($"{name}: {e.Message}", e);
            }
        }

        private ISimSweepGenerator RequireGenerator()
        {
            return generator ?? throw new InvalidOperationException("組み合わせの作り方が選ばれていません");
        }

        private string AxisRowName(int index) => $"{name} の Axes {index + 1} 行目";

        private string GroupRowName(int index) => $"{name} の Groups {index + 1} 行目";
    }

    public sealed class SimSweepChoice
    {
        public readonly IReadOnlyList<SimValueRow> Rows;
        public readonly SimCaseValue Label;

        public SimSweepChoice(IReadOnlyList<SimValueRow> rows, SimCaseValue label)
        {
            Rows = rows;
            Label = label;
        }
    }

    public interface ISimSweepGenerator
    {
        string Description { get; }

        int CountCases(IReadOnlyList<int> choiceCounts);

        List<int[]> Generate(IReadOnlyList<int> choiceCounts);
    }

    public interface ISimAdaptiveSweepGenerator : ISimSweepGenerator
    {
        List<int[]> Next(IReadOnlyList<int> choiceCounts, IReadOnlyList<SimSweepOutcome> done, List<string> warnings, out string conclusion);
    }

    public sealed class SimSweepOutcome
    {
        public readonly int[] Pick;
        public readonly string Label;
        public readonly SimResult Result;

        public SimSweepOutcome(int[] pick, string label, SimResult result)
        {
            Pick = pick;
            Label = label;
            Result = result;
        }

        public double ClearRate => Result.runCount > 0 ? 100.0 * Result.clearedCount / Result.runCount : 0;
    }

    [Serializable]
    public class GridSweep : ISimSweepGenerator
    {
        public const int MaxCases = 10000;

        public string Description => "格子: 全行の値の全組み合わせを試す。上の行ほどゆっくり変わる順に並ぶ";

        public int CountCases(IReadOnlyList<int> choiceCounts)
        {
            long count = 1;
            foreach (int choices in choiceCounts)
            {
                count *= choices;
                if (count > MaxCases)
                {
                    throw new InvalidOperationException($"組み合わせが {MaxCases} 通りを超えます。刻みを広げるか、行を減らしてください");
                }
            }
            return (int)count;
        }

        public List<int[]> Generate(IReadOnlyList<int> choiceCounts)
        {
            int total = CountCases(choiceCounts);
            var cases = new List<int[]>(total);
            for (int n = 0; n < total; n++)
            {
                var pick = new int[choiceCounts.Count];
                int rest = n;
                for (int d = choiceCounts.Count - 1; d >= 0; d--)
                {
                    pick[d] = rest % choiceCounts[d];
                    rest /= choiceCounts[d];
                }
                cases.Add(pick);
            }
            return cases;
        }
    }

    [Serializable]
    public class OneAtATimeSweep : ISimSweepGenerator
    {
        public string Description => "1行ずつ: 最初に、振る値を何も変えない基準の1通りを試す。続けて、1行だけを最小から最大まで動かし、他の行は変えない通りを試す。通り数は 1 + 各行の通り数の合計";

        public int CountCases(IReadOnlyList<int> choiceCounts)
        {
            return 1 + choiceCounts.Sum();
        }

        public List<int[]> Generate(IReadOnlyList<int> choiceCounts)
        {
            var cases = new List<int[]> { Unchanged(choiceCounts.Count) };
            for (int d = 0; d < choiceCounts.Count; d++)
            {
                for (int choice = 0; choice < choiceCounts[d]; choice++)
                {
                    int[] pick = Unchanged(choiceCounts.Count);
                    pick[d] = choice;
                    cases.Add(pick);
                }
            }
            return cases;
        }

        private static int[] Unchanged(int dimensions)
        {
            return Enumerable.Repeat(-1, dimensions).ToArray();
        }
    }

    [Serializable]
    public class RandomSweep : ISimSweepGenerator
    {
        private const int RepairRounds = 1000;

        [Tooltip("試す組み合わせの数")]
        [SerializeField] private int caseCount = 100;

        [Tooltip("組み合わせを選ぶ乱数のシード。行とシードが同じなら同じ組み合わせになる")]
        [SerializeField] private int seed = 1;

        public string Description => "ランダム: 各行の候補（最小から最大まで刻みずつ）から、全行を同時にばらばらに選んだ組み合わせを、指定した数だけ試す。行ごとに、どの候補もなるべく同じ回数ずつ出るように選ぶ";

        public int CountCases(IReadOnlyList<int> choiceCounts)
        {
            if (caseCount <= 0)
            {
                throw new InvalidOperationException("Case Count が 0 以下です");
            }

            long total = 1;
            foreach (int choices in choiceCounts)
            {
                total *= choices;
                if (total > caseCount)
                {
                    return caseCount;
                }
            }
            throw new InvalidOperationException($"全組み合わせが {total} 通りで、Case Count（{caseCount}）以下です。格子（GridSweep）で全部試してください");
        }

        public List<int[]> Generate(IReadOnlyList<int> choiceCounts)
        {
            CountCases(choiceCounts);
            var random = new System.Random(seed);
            var columns = new int[choiceCounts.Count][];
            for (int d = 0; d < choiceCounts.Count; d++)
            {
                int[] order = Enumerable.Range(0, choiceCounts[d]).ToArray();
                Shuffle(order, random);
                columns[d] = Enumerable.Range(0, caseCount).Select(i => order[i % order.Length]).ToArray();
                Shuffle(columns[d], random);
            }

            RepairDuplicates(columns, choiceCounts, random);
            return Enumerable.Range(0, caseCount).Select(i => columns.Select(column => column[i]).ToArray()).ToList();
        }

        private void RepairDuplicates(int[][] columns, IReadOnlyList<int> choiceCounts, System.Random random)
        {
            int[] variable = Enumerable.Range(0, choiceCounts.Count).Where(d => choiceCounts[d] > 1).ToArray();
            var uses = new Dictionary<string, int>();
            for (int i = 0; i < caseCount; i++)
            {
                Add(uses, Key(columns, i), 1);
            }

            for (int round = 0; round < RepairRounds; round++)
            {
                List<int> duplicates = Enumerable.Range(0, caseCount).Where(i => uses[Key(columns, i)] > 1).ToList();
                if (duplicates.Count == 0)
                {
                    return;
                }

                foreach (int i in duplicates)
                {
                    string before = Key(columns, i);
                    if (uses[before] <= 1)
                    {
                        continue;
                    }

                    int[] column = columns[variable[random.Next(variable.Length)]];
                    int j = random.Next(caseCount);
                    string beforeJ = Key(columns, j);
                    (column[i], column[j]) = (column[j], column[i]);
                    string after = Key(columns, i);
                    string afterJ = Key(columns, j);

                    int distinct = uses.Count;
                    Add(uses, before, -1);
                    Add(uses, beforeJ, -1);
                    Add(uses, after, 1);
                    Add(uses, afterJ, 1);
                    if (uses.Count < distinct)
                    {
                        Add(uses, after, -1);
                        Add(uses, afterJ, -1);
                        Add(uses, before, 1);
                        Add(uses, beforeJ, 1);
                        (column[i], column[j]) = (column[j], column[i]);
                    }
                }
            }
        }

        private static string Key(int[][] columns, int index)
        {
            return string.Join(",", columns.Select(column => column[index]));
        }

        private static void Add(Dictionary<string, int> uses, string key, int delta)
        {
            uses.TryGetValue(key, out int count);
            count += delta;
            if (count > 0)
            {
                uses[key] = count;
            }
            else
            {
                uses.Remove(key);
            }
        }

        private static void Shuffle(int[] values, System.Random random)
        {
            for (int i = values.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }
        }
    }

    [Serializable]
    public class TargetClearRateSweep : ISimAdaptiveSweepGenerator
    {
        [Tooltip("目指すクリア率（%）")]
        [SerializeField] private double targetClearRate = 50;

        [Tooltip("クリア率が目標からこの幅（ポイント）以内の値が出たら止める")]
        [SerializeField] private double tolerance = 2;

        [Tooltip("1回に試す値の数。1回目は最小と最大を含めて等間隔に選ぶ")]
        [SerializeField] private int pointsPerRound = 5;

        public string Description => "目標のクリア率: 振る行を1行だけにして使う。最小から最大まで等間隔に試し、クリア率が目標をまたぐ区間の内側をまた等間隔に試す、を繰り返す。目標から許容幅以内の値が出るか、刻みより細かく絞れなくなったら止める。何通りで止まるかは実行するまで分からない";

        public int CountCases(IReadOnlyList<int> choiceCounts)
        {
            Validate(choiceCounts);
            List<int> first = FirstRound(choiceCounts[0]);
            var memo = new Dictionary<int, int>();
            return first.Count + Enumerable.Range(0, first.Count - 1).Select(i => Worst(first[i + 1] - first[i], memo)).DefaultIfEmpty(0).Max();
        }

        public List<int[]> Generate(IReadOnlyList<int> choiceCounts)
        {
            Validate(choiceCounts);
            return FirstRound(choiceCounts[0]).Select(i => new[] { i }).ToList();
        }

        public List<int[]> Next(IReadOnlyList<int> choiceCounts, IReadOnlyList<SimSweepOutcome> done, List<string> warnings, out string conclusion)
        {
            Validate(choiceCounts);
            List<SimSweepOutcome> tried = done.Where(o => o.Pick != null).OrderBy(o => o.Pick[0]).ToList();
            if (tried.Count == 0)
            {
                throw new InvalidOperationException("試した値の結果がありません");
            }

            SimSweepOutcome best = tried.OrderBy(Distance).First();
            string bestText = $"一番近いのは {best.Label}（クリア率 {best.ClearRate:0.#}%）";
            if (Distance(best) <= tolerance)
            {
                conclusion = $"目標のクリア率 {targetClearRate:0.#}%（±{tolerance:0.#}）に入る値が見つかりました。{bestText}";
                return new List<int[]>();
            }

            List<int> brackets = Enumerable.Range(0, tried.Count - 1)
                .Where(i => IsAbove(tried[i]) != IsAbove(tried[i + 1]))
                .ToList();
            if (brackets.Count == 0)
            {
                conclusion = $"最小から最大までのどの値でも、クリア率が目標 {targetClearRate:0.#}% より{(IsAbove(tried[0]) ? "高い" : "低い")}ため、範囲内に目標の値がありません。範囲を広げてください。{bestText}";
                return new List<int[]>();
            }
            if (brackets.Count > 1)
            {
                warnings.Add($"クリア率が目標をまたぐ区間が {brackets.Count} か所あります。値に対してクリア率が一方向に変わっていないか、回数が少なくてぶれている可能性があります。目標に一番近い値を含む区間を絞り込みます");
            }

            int chosen = brackets.OrderBy(i => Math.Min(Distance(tried[i]), Distance(tried[i + 1]))).First();
            SimSweepOutcome low = tried[chosen];
            SimSweepOutcome high = tried[chosen + 1];
            List<int> next = Inside(low.Pick[0], high.Pick[0]);
            if (next.Count == 0)
            {
                conclusion = $"刻みより細かく絞れないため止めました。目標のクリア率 {targetClearRate:0.#}% は {low.Label} と {high.Label} の間。{bestText}";
                return new List<int[]>();
            }

            conclusion = null;
            return next.Select(i => new[] { i }).ToList();
        }

        private void Validate(IReadOnlyList<int> choiceCounts)
        {
            if (choiceCounts.Count != 1)
            {
                throw new InvalidOperationException($"振る行（Axes と Groups の合計）を1行だけにしてください（今は {choiceCounts.Count} 行）");
            }
            if (choiceCounts[0] < 2)
            {
                throw new InvalidOperationException("振る行の候補が1つだけです。最小と最大を変えてください");
            }
            if (pointsPerRound < 2)
            {
                throw new InvalidOperationException("Points Per Round が 2 未満です");
            }
            if (targetClearRate < 0 || targetClearRate > 100)
            {
                throw new InvalidOperationException("Target Clear Rate が 0〜100 の外です");
            }
            if (tolerance < 0)
            {
                throw new InvalidOperationException("Tolerance が負です");
            }
        }

        private double Distance(SimSweepOutcome outcome) => Math.Abs(outcome.ClearRate - targetClearRate);

        private bool IsAbove(SimSweepOutcome outcome) => outcome.ClearRate > targetClearRate;

        private List<int> FirstRound(int count)
        {
            if (count <= pointsPerRound)
            {
                return Enumerable.Range(0, count).ToList();
            }
            return Enumerable.Range(0, pointsPerRound)
                .Select(i => (int)Math.Round((double)i * (count - 1) / (pointsPerRound - 1)))
                .ToList();
        }

        private List<int> Inside(int low, int high)
        {
            int gap = high - low - 1;
            if (gap <= pointsPerRound)
            {
                return Enumerable.Range(low + 1, Math.Max(0, gap)).ToList();
            }
            return Enumerable.Range(1, pointsPerRound)
                .Select(j => low + (int)Math.Round((double)j * (high - low) / (pointsPerRound + 1)))
                .ToList();
        }

        private int Worst(int distance, Dictionary<int, int> memo)
        {
            int gap = distance - 1;
            if (gap <= pointsPerRound)
            {
                return Math.Max(0, gap);
            }
            if (memo.TryGetValue(distance, out int known))
            {
                return known;
            }

            List<int> points = Inside(0, distance);
            points.Insert(0, 0);
            points.Add(distance);
            int worst = pointsPerRound + Enumerable.Range(0, points.Count - 1).Max(i => Worst(points[i + 1] - points[i], memo));
            memo[distance] = worst;
            return worst;
        }
    }

    [Serializable]
    public class SimSweepAxis
    {
        public const string TargetIdField = nameof(targetId);
        public const string TargetLabelField = nameof(targetLabel);
        public const string PropertyPathField = nameof(propertyPath);
        public const string PropertyLabelField = nameof(propertyLabel);
        public const string MinField = nameof(min);
        public const string MaxField = nameof(max);
        public const string StepField = nameof(step);

        [SerializeField] private string targetId;
        [SerializeField] private string targetLabel;
        [SerializeField] private string propertyPath;
        [SerializeField] private string propertyLabel;
        [SerializeField] private double min;
        [SerializeField] private double max;
        [SerializeField] private double step;

        public static string Validate(double min, double max, double step)
        {
            if (max < min)
            {
                return "最大が最小より小さい";
            }
            if (max > min && step <= 0)
            {
                return "刻みが 0 以下";
            }
            if (max > min && (max - min) / step >= GridSweep.MaxCases)
            {
                return $"{GridSweep.MaxCases} 通りを超える";
            }
            return null;
        }

        public static List<double> ValuesOf(double min, double max, double step)
        {
            int count = max > min ? (int)Math.Floor((max - min) / step + 1e-9) + 1 : 1;
            return Enumerable.Range(0, count).Select(k => Math.Round(min + k * step, 10)).ToList();
        }

        public List<double> Values(string row)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new InvalidOperationException($"{row}: 対象が設定されていません");
            }
            if (string.IsNullOrEmpty(propertyPath))
            {
                throw new InvalidOperationException($"{row}（{targetLabel}）: 項目が選ばれていません");
            }

            string error = Validate(min, max, step);
            if (error != null)
            {
                throw new InvalidOperationException($"{row}（{targetLabel} の {propertyLabel}）: {error}");
            }
            return ValuesOf(min, max, step);
        }

        public List<SimSweepChoice> Choices(string row)
        {
            return Values(row).Select(v => new SimSweepChoice(
                new[] { new SimValueRow(row, new SimValueOverride(targetId, targetLabel, propertyPath, propertyLabel, v)) },
                new SimCaseValue { target = targetLabel, property = propertyLabel, value = v })).ToList();
        }
    }

    [Serializable]
    public class SimSweepGroup
    {
        public const string NameField = nameof(name);
        public const string TargetsField = nameof(targets);
        public const string MinField = nameof(min);
        public const string MaxField = nameof(max);
        public const string StepField = nameof(step);
        public const string FactorLabel = "倍率";

        [SerializeField] private string name;
        [SerializeField] private List<SimValueTarget> targets = new();
        [SerializeField] private double min = 1;
        [SerializeField] private double max = 1;
        [SerializeField] private double step = 0.1;

        public static string Validate(double min, double max, double step)
        {
            return min < 0 ? "倍率が負" : SimSweepAxis.Validate(min, max, step);
        }

        public List<double> Factors(string row)
        {
            string label = string.IsNullOrEmpty(name) ? row : $"{row}（{name}）";
            if (string.IsNullOrEmpty(name))
            {
                throw new InvalidOperationException($"{label}: 名前が設定されていません（結果の表の列名に使う）");
            }
            if (targets.Count == 0)
            {
                throw new InvalidOperationException($"{label}: 対象がありません");
            }
            for (int i = 0; i < targets.Count; i++)
            {
                if (string.IsNullOrEmpty(targets[i].TargetId))
                {
                    throw new InvalidOperationException($"{label} の {i + 1} 番目: 対象が設定されていません");
                }
                if (string.IsNullOrEmpty(targets[i].PropertyPath))
                {
                    throw new InvalidOperationException($"{label} の {i + 1} 番目（{targets[i].TargetLabel}）: 項目が選ばれていません");
                }
            }

            string error = Validate(min, max, step);
            if (error != null)
            {
                throw new InvalidOperationException($"{label}: {error}");
            }
            return SimSweepAxis.ValuesOf(min, max, step);
        }

        public List<SimSweepChoice> Choices(string row)
        {
            return Factors(row).Select(factor => new SimSweepChoice(
                targets.Select((t, i) => new SimValueRow($"{row} の {i + 1} 番目",
                    new SimValueOverride(t.TargetId, t.TargetLabel, t.PropertyPath, t.PropertyLabel, factor, true))).ToList(),
                new SimCaseValue { target = name, property = FactorLabel, value = factor })).ToList();
        }
    }

    [Serializable]
    public class SimValueTarget
    {
        public const string TargetIdField = nameof(targetId);
        public const string TargetLabelField = nameof(targetLabel);
        public const string PropertyPathField = nameof(propertyPath);
        public const string PropertyLabelField = nameof(propertyLabel);

        [SerializeField] private string targetId;
        [SerializeField] private string targetLabel;
        [SerializeField] private string propertyPath;
        [SerializeField] private string propertyLabel;

        public string TargetId => targetId;
        public string TargetLabel => targetLabel;
        public string PropertyPath => propertyPath;
        public string PropertyLabel => propertyLabel;
    }
}
