namespace BalanceSim
{
    public static class Simulator
    {
        public static SimResult Run(SimInput input)
        {
            return new SimResult
            {
                message = $"Phase {input.phases.Count}件、実行回数 {input.runCount} を受け取りました",
                input = input,
            };
        }
    }
}
