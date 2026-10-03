using UnityEditor;
using UnityEngine;

namespace BalanceSim.Editor
{
    [CreateAssetMenu(fileName = "BalanceSimSettings", menuName = "BalanceSim/Settings")]
    public class BalanceSimSettings : ScriptableObject
    {
        [Header("実行")]
        [Tooltip("シミュレーションを回す回数。探索SO のときは、組み合わせ1通りごとにこの回数ずつ回す")]
        [Min(1)]
        [SerializeField] private int runCount = 100;
        public int RunCount => runCount;

        [Tooltip("乱数のシード。同じ値なら同じ結果になる")]
        [SerializeField] private int seed;
        public int Seed => seed;

        [Tooltip("値を読み出すシーン。開いていなければ一時的に追加で開き、読み終えたら閉じる")]
        [SerializeField] private SceneAsset targetScene;
        public SceneAsset TargetScene => targetScene;

        [Header("ゲームの値の上書き")]
        [Tooltip("ゲームの値を上書きする数値SO（Assets > Create > BalanceSim > Values）、または値を振って試す探索SO（Assets > Create > BalanceSim > Sweep）。空ならゲームの値のまま実行する。アセットやシーンは書き換えず、読み出しの間だけ値を差し替える")]
        [SerializeField] private SimValueSource values;
        public SimValueSource Values => values;

        [Header("時間")]
        [Tooltip("1ステップの秒数。ゲームの物理（Fixed Timestep）と同じ 0.02 が既定")]
        [Min(0.001f)]
        [SerializeField] private float timeStep = 0.02f;
        public float TimeStep => timeStep;

        [Tooltip("結果の時系列を記録する間隔（秒）")]
        [Min(0.01f)]
        [SerializeField] private float sampleInterval = 1f;
        public float SampleInterval => sampleInterval;

        [Header("プレイヤーの仮定")]
        [Tooltip("プレイヤーの向き。敵の出現方向の基準になる")]
        [SerializeField] private SimFacingMode facing = SimFacingMode.MoveDirection;
        public SimFacingMode Facing => facing;

        [Tooltip("逃げる方向を決めるときに数える敵の範囲（m）")]
        [Min(1f)]
        [SerializeField] private float escapeRadius = 30f;
        public float EscapeRadius => escapeRadius;

        [Tooltip("その方向にどこまで通れるかを調べる距離（m）。袋小路を避けるのに使う")]
        [Min(1f)]
        [SerializeField] private float escapeLookahead = 15f;
        public float EscapeLookahead => escapeLookahead;

        [Tooltip("先が行き止まりの方向をどれだけ避けるか。0 で行き止まりを気にしない")]
        [Min(0f)]
        [SerializeField] private float escapeDeadEndWeight = 1f;
        public float EscapeDeadEndWeight => escapeDeadEndWeight;

        [Tooltip("今の進行方向から向きを変えることをどれだけ嫌うか。0 で毎ステップ自由に向きを変える")]
        [Min(0f)]
        [SerializeField] private float escapeTurnWeight = 0.2f;
        public float EscapeTurnWeight => escapeTurnWeight;

        [Tooltip("照準のぶれ（度）。手動・自動を問わず、カメラの向きで撃つ武器は撃つたびに ±この角度の範囲で一様にずれる。武器自身の拡散（spreadAngle）とは別に足される")]
        [Min(0f)]
        [SerializeField] private float aimError = 2f;
        public float AimError => aimError;

        [Tooltip("手動の武器（メイン・アビリティ）を撃ち始める距離（m）。一番近い敵がこの距離より遠ければ撃たない")]
        [Min(0f)]
        [SerializeField] private float fireRange = 60f;
        public float FireRange => fireRange;

        [Header("地形")]
        [Tooltip("通れる/通れないを調べる格子の一辺（m）")]
        [Min(0.1f)]
        [SerializeField] private float terrainCellSize = 0.5f;
        public float TerrainCellSize => terrainCellSize;

        [Header("レベルアップの選び方")]
        [Tooltip("3つの選択肢から1つを選ぶ方法。IUpgradeChoicePolicy を実装したクラスを足すと一覧に出る")]
        [SerializeReference] private IUpgradeChoicePolicy choicePolicy = new RandomChoicePolicy();
        public IUpgradeChoicePolicy ChoicePolicy => choicePolicy;
    }
}
