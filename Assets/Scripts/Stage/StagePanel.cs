using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace InfiniteTileWorld
{
    /// <summary>
    /// 無限ワールドの個別タイル。プレイヤー進入検知とワープ後コンテンツ再配置を担う。
    /// </summary>
    /// <remarks>
    /// 拡張方法: このクラスを継承して OnWarped() をオーバーライドする。
    ///   public class ForestPanel : StagePanel {
    ///       public override void OnWarped(Vector3 pos, float size) {
    ///           base.OnWarped(pos, size);
    ///           SpawnTrees();
    ///       }
    ///   }
    /// </remarks>
    public class StagePanel : MonoBehaviour
    {
        /// <summary>ワープ時にパネル内ランダム座標へ再配置されるオブジェクト。</summary>
        [SerializeField] private List<GameObject> contentObjects = new();

        [Header("Randomization Settings")]
        /// <summary>出現率 (0.0 ～ 1.0)。</summary>
        [Range(0f, 1f)] [SerializeField] private float appearanceRate = 1.0f;
        /// <summary>ランダムに回転（Y軸）させるかどうか。</summary>
        [SerializeField] private bool randomYRotation = true;

        /// <summary>XZ軸の配置範囲（ローカル座標）。パネル上が 0.4 の場合は (-0.4, 0.4)。</summary>
        [SerializeField] private Vector2 xzRange = new Vector2(-0.4f, 0.4f);
        /// <summary>Y軸（高さ）の配置範囲（ローカル座標）。</summary>
        [SerializeField] private Vector2 yRange = new Vector2(0f, 0.2f);

        /// <summary>ワープ完了後に発火する UnityEvent。</summary>
        [SerializeField] private UnityEvent onAfterWarp;

        [SerializeField] private string playerTag = "Player";

        [SerializeField] private float moveHeight = 3f;
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Enemy Relocation")]
        /// <summary>移動したタイル上に敵を移動させるか</summary>
        [SerializeField] private bool relocateEnemiesOnWarp = true;
        /// <summary>移動させる敵の上下範囲</summary>
        [SerializeField, Min(1f)] private float enemyRelocationHalfHeight = 50f;

        public Vector3 WorldCenter { get; private set; }

        private StageManager _manager;
        private Collider _panelCollider;

        /// <summary>Enemyの種類と座標を保持した構造体</summary>
        /// <summary>
        /// ワープ前のパネルから取得した、移送対象の敵とパネル基準の相対位置。
        /// </summary>
        private readonly struct EnemyRelocation
        {
            public Enemy Enemy { get; }
            public Vector3 LocalOffset { get; }

            public EnemyRelocation(Enemy enemy, Vector3 localOffset)
            {
                Enemy = enemy;
                LocalOffset = localOffset;
            }
        }

        private void Awake()
        {
            _panelCollider = GetComponent<Collider>();
        }

        public void Initialize(StageManager manager)
        {
            _manager = manager;
            WorldCenter = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;
            _manager?.OnPlayerEntered(this);
        }

        /// <summary>
        /// StageManager からワープ実行時に呼び出される。継承クラスでオーバーライド可能。
        /// </summary>
        /// <param name="newWorldPosition">移動先のワールド座標。</param>
        /// <param name="tileSize">コンテンツ散布範囲の算出に使用するタイルサイズ。</param>
        public virtual void OnWarped(Vector3 newWorldPosition, float tileSize)
        {
            // 実行中のパネル移動を止め、ワープ前の状態から移送対象を確定する。
            StopAllCoroutines();
            List<EnemyRelocation> enemies = CaptureEnemiesOnPanel();

            // 地面の移動完了後に、記録した敵を新しいパネルへ移す。
            StartCoroutine(WarpCoroutine(newWorldPosition, tileSize, enemies));
        }

        /// <summary>contentObjects をパネル内 XZ ランダム座標へ移動し、向きや表示もランダム化する。</summary>
        protected void ScatterContents(float tileSize)
        {
            foreach (var obj in contentObjects)
            {
                if (obj == null) continue;

                // 出現判定
                bool show = Random.value <= appearanceRate;
                obj.SetActive(show);
                if (!show) continue;

                // パネル内のランダムな相対座標を決定
                float rx = Random.Range(xzRange.x, xzRange.y);
                float rz = Random.Range(xzRange.x, xzRange.y);
                float ry = Random.Range(yRange.x, yRange.y);

                // 接地判定（ワールド座標でレイを飛ばす）
                float groundY = 0f;
                // ローカル座標 (rx, 0, rz) をワールド座標に変換してレイの開始地点にする
                Vector3 worldPos = transform.TransformPoint(new Vector3(rx, 0, rz));
                Vector3 worldRayOrigin = worldPos + Vector3.up * 50f;

                if (Physics.Raycast(worldRayOrigin, Vector3.down, out RaycastHit hit, 100f))
                {
                    // ヒットした位置をパネルからの相対座標に変換
                    groundY = transform.InverseTransformPoint(hit.point).y;
                }

                // ローカル座標で位置を設定（地面の高さ + ランダムな高さオフセット）
                obj.transform.localPosition = new Vector3(rx, groundY + ry, rz);

                // ランダム回転
                if (randomYRotation)
                {
                    obj.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                }
            }
        }

        /// <summary>contentObjects をパネル内 XZ ランダム座標へ移動し、向きや表示もランダム化する。</summary>
        /// <summary>
        /// パネルのワープ開始時に、その水平範囲内にいる敵を移送対象として記録する。
        /// </summary>
        private List<EnemyRelocation> CaptureEnemiesOnPanel()
        {
            var enemies = new List<EnemyRelocation>();
            bool canRelocateEnemies = relocateEnemiesOnWarp && _panelCollider != null;
            if (!canRelocateEnemies)
            {
                return enemies;
            }

            // パネルの XZ 範囲を使い、上下方向だけ設定値まで検出範囲を拡張する。
            Bounds bounds = _panelCollider.bounds;
            Vector3 halfExtents = bounds.extents;
            halfExtents.y = enemyRelocationHalfHeight;

            // ワープ時だけ物理クエリを行い、パネル上空・下方を含む敵 Collider を取得する。
            Collider[] colliders = Physics.OverlapBox(
                bounds.center,
                halfExtents,
                Quaternion.identity,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide);

            var captured = new HashSet<Enemy>();
            foreach (Collider other in colliders)
            {
                Enemy enemy = other.GetComponentInParent<Enemy>();
                if (enemy == null)
                {
                    continue;
                }

                // 1 体に複数の Collider があっても、移送対象には一度だけ追加する。
                bool isNewEnemy = captured.Add(enemy);
                if (!isNewEnemy)
                {
                    continue;
                }

                // ワープ後もパネル内での水平位置を保てるよう、パネル基準の差分を記録する。
                enemies.Add(new EnemyRelocation(
                    enemy,
                    enemy.transform.position - transform.position));
            }

            return enemies;
        }

        private IEnumerator WarpCoroutine(
            Vector3 newWorldPosition,
            float tileSize,
            List<EnemyRelocation> enemies)
        {
            // 消える
            yield return MoveCoroutine(transform.position, transform.position - Vector3.up * moveHeight);

            // ワープ
            transform.position = newWorldPosition;

            // コンテンツ配置
            ScatterContents(tileSize);

            // 出現
            yield return MoveCoroutine(newWorldPosition - Vector3.up * moveHeight, newWorldPosition);

            // 地面が最終位置に戻ってから敵を配置し、移送中の落下を防ぐ。
            RelocateEnemies(enemies);
            onAfterWarp?.Invoke();
        }

        /// <summary>
        /// 地面パネルのワープ完了後、記録済みの敵を新しいパネル上へ移送する。
        /// </summary>
        private void RelocateEnemies(List<EnemyRelocation> enemies)
        {
            foreach (EnemyRelocation relocation in enemies)
            {
                Enemy enemy = relocation.Enemy;
                bool isActiveEnemy = enemy != null && enemy.gameObject.activeInHierarchy;
                if (!isActiveEnemy)
                {
                    continue;
                }

                // XZ はワープ前のパネル内位置を維持し、Y は新しい地面の上面に合わせる。
                Vector3 destination = transform.position + relocation.LocalOffset;
                destination.y = CalculateGroundedEnemyY(enemy);

                // Rigidbody を持つ敵は物理座標と残留速度を更新する。
                bool hasRigidbody = enemy.TryGetComponent(out EnemyBrain brain) && brain.Rb != null;
                if (hasRigidbody)
                {
                    brain.Rb.position = destination;
                    brain.Rb.linearVelocity = Vector3.zero;
                    brain.Rb.angularVelocity = Vector3.zero;
                }
                else
                {
                    enemy.transform.position = destination;
                }
            }
        }

        /// <summary>
        /// 敵の Collider 下端をパネル上面に合わせるための、移送先 Y 座標を計算する。
        /// </summary>
        private float CalculateGroundedEnemyY(Enemy enemy)
        {
            // 複数 Collider を持つ敵にも対応するため、最も低い下端を足元として扱う。
            float lowestPoint = enemy.transform.position.y;
            foreach (Collider collider in enemy.GetComponentsInChildren<Collider>())
            {
                lowestPoint = Mathf.Min(lowestPoint, collider.bounds.min.y);
            }

            // 敵 Transform から足元までの距離を保ち、パネル上面の少し上へ配置する。
            float feetOffset = enemy.transform.position.y - lowestPoint;
            return _panelCollider.bounds.max.y + feetOffset + 0.02f;
        }

        private IEnumerator MoveCoroutine(Vector3 startPos, Vector3 endPos)
        {
            float t = 0f;

            while (t < moveDuration)
            {
                t += Time.deltaTime;

                float rate = Mathf.Clamp01(t / moveDuration);
                float curve = moveCurve.Evaluate(rate);

                transform.position = Vector3.LerpUnclamped(startPos, endPos, curve);

                yield return null;
            }

            transform.position = endPos;
        }


    }
}
