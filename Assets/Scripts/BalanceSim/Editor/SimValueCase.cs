using System.Collections.Generic;
using System.Linq;

namespace BalanceSim.Editor
{
    public sealed class SimValueRow
    {
        public readonly string Row;
        public readonly SimValueOverride Value;

        public SimValueRow(string row, SimValueOverride value)
        {
            Row = row;
            Value = value;
        }
    }

    public sealed class SimValueCase
    {
        public readonly IReadOnlyList<SimValueRow> Fixed;
        public readonly IReadOnlyList<SimValueRow> Swept;

        public SimValueCase(IReadOnlyList<SimValueRow> fixedRows, IReadOnlyList<SimValueRow> sweptRows)
        {
            Fixed = fixedRows;
            Swept = sweptRows;
        }

        public static SimValueCase None => new(new List<SimValueRow>(), new List<SimValueRow>());

        public IEnumerable<SimValueRow> Rows => Fixed.Concat(Swept);
    }
}
