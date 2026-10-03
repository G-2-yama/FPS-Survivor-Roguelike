using System;
using System.Collections.Generic;

namespace BalanceSim
{
    [Serializable]
    public class SimResult
    {
        public string message;
        public float sampleInterval;
        public List<SimSeries> series = new();
        public List<SimRunSummary> runs = new();
    }

    [Serializable]
    public class SimSeries
    {
        public string name;
        public string group;
        public bool ratio;
        public List<float> mean = new();
        public List<float> p10 = new();
        public List<float> p50 = new();
        public List<float> p90 = new();
    }

    [Serializable]
    public class SimRunSummary
    {
        public int seed;
        public bool cleared;
        public float endTime;
        public int level;
        public int kills;
        public List<SimLevelUpRecord> levelUps = new();
        public List<SimEvolutionRecord> evolutions = new();
    }

    [Serializable]
    public class SimEvolutionRecord
    {
        public float time;
        public string weapon;
    }

    [Serializable]
    public class SimLevelUpRecord
    {
        public float time;
        public int level;
        public string chosen;
        public List<string> options = new();
    }
}
