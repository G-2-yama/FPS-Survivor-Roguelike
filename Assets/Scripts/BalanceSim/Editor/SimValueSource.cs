using System.Collections.Generic;
using UnityEngine;

namespace BalanceSim.Editor
{
    public abstract class SimValueSource : ScriptableObject
    {
        public abstract List<SimValueCase> BuildCases();
    }
}
