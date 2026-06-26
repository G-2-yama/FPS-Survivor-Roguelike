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

        public Vector3 WorldCenter { get; private set; }

        private StageManager _manager;

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
            StopAllCoroutines();
            StartCoroutine(WarpCoroutine(newWorldPosition, tileSize));
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

        private IEnumerator WarpCoroutine(Vector3 newWorldPosition, float tileSize)
        {
            // 消える
            yield return MoveCoroutine(transform.position, transform.position - Vector3.up * moveHeight);

            // ワープ
            transform.position = newWorldPosition;

            // コンテンツ配置
            ScatterContents(tileSize);

            // 出現
            yield return MoveCoroutine(newWorldPosition - Vector3.up * moveHeight, newWorldPosition);

            onAfterWarp?.Invoke();
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
