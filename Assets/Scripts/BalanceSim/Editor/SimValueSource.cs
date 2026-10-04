using System.Collections.Generic;
using UnityEngine;

namespace BalanceSim.Editor
{
    public abstract class SimValueSource : ScriptableObject
    {
        public abstract List<SimValueCase> BuildCases();

        public virtual List<SimValueCase> BuildNextCases(IReadOnlyList<SimValueCase> done, IReadOnlyList<SimResult> results,
            List<string> warnings, out string conclusion)
        {
            conclusion = null;
            return new List<SimValueCase>();
        }
    }
}
