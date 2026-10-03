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

        [Tooltip("振る値の組み合わせの作り方。ISimSweepGenerator を実装したクラスを足すと一覧に出る")]
        [SerializeReference] private ISimSweepGenerator generator = new GridSweep();
        public ISimSweepGenerator Generator => generator;

        public int CountCases()
        {
            return RequireGenerator().CountCases(name);
        }

        public override List<SimValueCase> BuildCases()
        {
            List<SimValueRow> fixedRows = fixedValues != null ? fixedValues.Rows() : new List<SimValueRow>();
            return RequireGenerator().Generate(name).Select(swept => new SimValueCase(fixedRows, swept)).ToList();
        }

        private ISimSweepGenerator RequireGenerator()
        {
            return generator ?? throw new InvalidOperationException($"{name}: 組み合わせの作り方が選ばれていません");
        }
    }

    public interface ISimSweepGenerator
    {
        int CountCases(string sweepName);

        List<List<SimValueRow>> Generate(string sweepName);
    }

    [Serializable]
    public class GridSweep : ISimSweepGenerator
    {
        public const int MaxCases = 10000;

        [Tooltip("振る値。行ごとに最小から最大まで刻みずつ振り、全行の値の全組み合わせを試す。上の行ほどゆっくり変わる順に並ぶ")]
        [SerializeField] private List<SimSweepAxis> axes = new();

        public int CountCases(string sweepName)
        {
            RequireAxes(sweepName);
            long count = 1;
            for (int i = 0; i < axes.Count; i++)
            {
                count *= axes[i].Values(RowName(sweepName, i)).Count;
                if (count > MaxCases)
                {
                    throw new InvalidOperationException($"{sweepName}: 組み合わせが {MaxCases} 通りを超えます。刻みを広げるか、行を減らしてください");
                }
            }
            return (int)count;
        }

        public List<List<SimValueRow>> Generate(string sweepName)
        {
            CountCases(sweepName);
            var cases = new List<List<SimValueRow>> { new() };
            for (int i = 0; i < axes.Count; i++)
            {
                string row = RowName(sweepName, i);
                List<SimValueOverride> values = axes[i].Overrides(row);
                cases = cases.SelectMany(c => values.Select(v => c.Append(new SimValueRow(row, v)).ToList())).ToList();
            }
            return cases;
        }

        private void RequireAxes(string sweepName)
        {
            if (axes.Count == 0)
            {
                throw new InvalidOperationException($"{sweepName}: 振る値の行がありません");
            }
        }

        private static string RowName(string sweepName, int index) => $"{sweepName} の {index + 1} 行目";
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

        public List<SimValueOverride> Overrides(string row)
        {
            return Values(row).Select(v => new SimValueOverride(targetId, targetLabel, propertyPath, propertyLabel, v)).ToList();
        }
    }
}
