using UnityEngine;

namespace BalanceSim.Editor
{
    [CreateAssetMenu(fileName = "BalanceSimSettings", menuName = "BalanceSim/Settings")]
    public class BalanceSimSettings : ScriptableObject
    {
        [Header("実行")]
        [Tooltip("シミュレーションを回す回数")]
        [Min(1)]
        [SerializeField] private int runCount = 100;
        public int RunCount => runCount;

        [Tooltip("乱数のシード。同じ値なら同じ結果になる")]
        [SerializeField] private int seed;
        public int Seed => seed;
    }
}
