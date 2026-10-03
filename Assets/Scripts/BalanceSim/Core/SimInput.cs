using System;
using System.Collections.Generic;

namespace BalanceSim
{
    [Serializable]
    public class SimInput
    {
        public int runCount;
        public int seed;
        public List<SimPhase> phases = new();
    }

    [Serializable]
    public class SimPhase
    {
        public string name;
        public float startTime;
        public int minSpawnCount;
        public float spawnInterval;
        public List<SimSpawnEntry> enemies = new();
    }

    [Serializable]
    public class SimSpawnEntry
    {
        public string prefabName;
        public int spawnWeight;
    }
}
