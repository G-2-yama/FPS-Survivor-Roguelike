using System;
using System.Collections.Generic;

namespace BalanceSim
{
    public interface IUpgradeChoicePolicy
    {
        int Choose(UpgradeChoiceContext context);
    }

    public sealed class UpgradeChoiceContext
    {
        public IReadOnlyList<SimUpgradeDef> Options { get; set; }
        public float Time { get; set; }
        public int Level { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public IReadOnlyList<string> Weapons { get; set; }
        public IReadOnlyList<string> Items { get; set; }
        public Random Random { get; set; }
    }

    [Serializable]
    public class RandomChoicePolicy : IUpgradeChoicePolicy
    {
        public int Choose(UpgradeChoiceContext context)
        {
            return context.Random.Next(context.Options.Count);
        }
    }

    [Serializable]
    public class PriorityChoicePolicy : IUpgradeChoicePolicy
    {
        public List<string> priority = new();

        public int Choose(UpgradeChoiceContext context)
        {
            int best = -1;
            int bestRank = int.MaxValue;
            for (int i = 0; i < context.Options.Count; i++)
            {
                int rank = priority.IndexOf(context.Options[i].name);
                if (rank >= 0 && rank < bestRank)
                {
                    best = i;
                    bestRank = rank;
                }
            }
            return best >= 0 ? best : context.Random.Next(context.Options.Count);
        }
    }
}
