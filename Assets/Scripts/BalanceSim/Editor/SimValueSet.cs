using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BalanceSim.Editor
{
    [CreateAssetMenu(fileName = "BalanceSimValues", menuName = "BalanceSim/Values")]
    public class SimValueSet : SimValueSource
    {
        [Tooltip("上書きする値の一覧。アセットやシーンの値は書き換えず、シミュレーションの読み出しの間だけ差し替える。同じ項目が複数あるときは下の行が勝つ")]
        [SerializeField] private List<SimValueOverride> overrides = new();
        public IReadOnlyList<SimValueOverride> Overrides => overrides;

        public override List<SimValueCase> BuildCases()
        {
            return new List<SimValueCase> { new(Rows(), new List<SimValueRow>()) };
        }

        public List<SimValueRow> Rows()
        {
            return overrides.Select((entry, i) => new SimValueRow($"{name} の {i + 1} 行目", entry)).ToList();
        }
    }
}
