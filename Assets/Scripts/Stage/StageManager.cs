using System.Collections.Generic;
using UnityEngine;

namespace InfiniteTileWorld
{
    /// <summary>
    /// gridSize×gridSizeのStagePanelを循環再利用する無限ワールドの中央司令塔。
    /// </summary>
    /// <remarks>
    /// セットアップ:
    /// 1. 空の GameObject にアタッチする。
    /// 2. インスペクタの「Setup Panels」でタイルを自動配置する。
    /// 3. Fog End（Lighting ウィンドウ）を fogEndDistance と同じ値にする。
    /// 4. プレイヤーに「Player」タグを付与する。
    ///
    /// Script Execution Order: LateUpdate でワープするため、
    /// Edit > Project Settings > Script Execution Order で +100 など大きな値を推奨。
    /// </remarks>
    public class StageManager : MonoBehaviour
    {
        [SerializeField] private float tileSize = 50f;

        /// <summary>Unity の Fog End と合わせること。この距離を超えたパネルをワープ候補にする。</summary>
        [SerializeField] private float fogEndDistance = 30f;

        /// <summary>ワープ確定に必要な視界外継続フレーム数。チャタリング防止用（デフォルト10≒0.17秒@60fps）。</summary>
        [SerializeField] private int warpConfirmFrames = 10;

        /// <summary>
        /// グリッドのサイズ
        /// </summary>
        /// <remarks>
        /// gridSize は奇数である必要があります
        /// </remarks>
        [SerializeField] private int gridSize = 5;
        public int GridSize => gridSize;

        /// <summary>gridSize×gridSizeのパネルリスト。「Setup Panels」で自動登録される。</summary>
        [SerializeField] private List<StagePanel> panels = new();
        [SerializeField] private BackgroundPanel backgroundPanel = new();

        /// <summary>距離判定カメラ。未設定時は Camera.main を使用。</summary>
        [SerializeField] private Transform cameraTransform;

        /// <summary>追従対象のプレイヤー。未設定時は "Player" タグのオブジェクトを使用。</summary>
        [SerializeField] private Transform playerTransform;

        // リングバッファオフセット（物理シフト不要、モジュロで解決）
        private int _gridOffsetX;
        private int _gridOffsetZ;
        private int _prevGridX;
        private int _prevGridZ;
        private readonly Dictionary<StagePanel, int> _warpQueue = new();

        public float TileSize => tileSize;
        public List<StagePanel> Panels => panels;

        private void Awake()
        {
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            foreach (var panel in panels)
                panel?.Initialize(this);
        }

        private void Start()
        {
            if (playerTransform == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) playerTransform = playerObj.transform;
            }

            if (playerTransform != null)
                (_prevGridX, _prevGridZ) = WorldToGrid(playerTransform.position);
        }

        private void LateUpdate()
        {
            if (playerTransform == null) return;

            // 背景パネルをプレイヤーの位置に追従させる
            Vector3 pos = backgroundPanel.transform.position;
            pos.x = playerTransform.position.x;
            pos.z = playerTransform.position.z;
            backgroundPanel.transform.position = pos;

            var (curX, curZ) = WorldToGrid(playerTransform.position);
            int dx = curX - _prevGridX;
            int dz = curZ - _prevGridZ;

            if (dx != 0 || dz != 0)
            {
                EnqueueOutOfBoundPanels(dx, dz);
                //backgroundPanel.OnWarped(curX, curZ, tileSize);
                _prevGridX = curX;
                _prevGridZ = curZ;
            }

            ProcessWarpQueue();
        }

        /// <summary>StagePanel がプレイヤーの進入を検知したときに呼び出す。</summary>
        public void OnPlayerEntered(StagePanel panel)
        {
            if (playerTransform == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) playerTransform = playerObj.transform;
            }
        }

        private (int gx, int gz) WorldToGrid(Vector3 worldPos)
        {
            // タイルの中心を基準にグリッド座標へ変換するため、半タイル分オフセットを加える
            return (Mathf.FloorToInt((worldPos.x + tileSize * 0.5f) / tileSize),
                    Mathf.FloorToInt((worldPos.z + tileSize * 0.5f) / tileSize));
        }

        private StagePanel GetPanel(int logicalX, int logicalZ)
        {
            // 剰余が負値にならないように補正する
            int px = ((_gridOffsetX + logicalX) % gridSize + gridSize) % gridSize;
            int pz = ((_gridOffsetZ + logicalZ) % gridSize + gridSize) % gridSize;
            int idx = pz * gridSize + px;
            return (idx >= 0 && idx < panels.Count) ? panels[idx] : null;
        }

        private void EnqueueOutOfBoundPanels(int dx, int dz)
        {
            var outdated = new HashSet<StagePanel>();

            // 移動方向に応じて、グリッドの端にあるパネルをワープ候補にする
            if (dx > 0)      for (int z = 0; z < gridSize; z++) AddIfNotNull(outdated, GetPanel(0, z));
            else if (dx < 0) for (int z = 0; z < gridSize; z++) AddIfNotNull(outdated, GetPanel(gridSize - 1, z));

            if (dz > 0)      for (int x = 0; x < gridSize; x++) AddIfNotNull(outdated, GetPanel(x, 0));
            else if (dz < 0) for (int x = 0; x < gridSize; x++) AddIfNotNull(outdated, GetPanel(x, gridSize - 1));

            foreach (var panel in outdated)
                if (!_warpQueue.ContainsKey(panel)) _warpQueue[panel] = 0;

            // オフセットを更新して論理的なグリッド位置を回転させる
            _gridOffsetX = ((_gridOffsetX + dx) % gridSize + gridSize) % gridSize;
            _gridOffsetZ = ((_gridOffsetZ + dz) % gridSize + gridSize) % gridSize;
        }

        private static void AddIfNotNull(HashSet<StagePanel> set, StagePanel panel)
        {
            if (panel != null) set.Add(panel);
        }

        private void ProcessWarpQueue()
        {
            if (cameraTransform == null) return;

            var toRemove  = new List<StagePanel>();
            var toWarp    = new List<StagePanel>();
            var toIncrement = new List<StagePanel>();

            foreach (var kvp in _warpQueue)
            {
                float dist = Vector3.Distance(cameraTransform.position, kvp.Key.WorldCenter);
                if (dist <= fogEndDistance)
                {
                    toRemove.Add(kvp.Key);
                }
                else if (kvp.Value + 1 >= warpConfirmFrames)
                {
                    toWarp.Add(kvp.Key);
                }
                else
                {
                    toIncrement.Add(kvp.Key);
                }
            }

            foreach (var p in toRemove)    _warpQueue.Remove(p);
            foreach (var p in toIncrement) _warpQueue[p]++;
            foreach (var p in toWarp)      { _warpQueue.Remove(p); ExecuteWarp(p); }
        }

        private void ExecuteWarp(StagePanel panel)
        {
            if (playerTransform == null) return;

            var (px, pz) = WorldToGrid(playerTransform.position);

            // 物理的なリスト内のインデックスを取得
            int physIdx = panels.IndexOf(panel);
            if (physIdx == -1) return;

            int physX = physIdx % gridSize;
            int physZ = physIdx / gridSize;

            // リングバッファのオフセットを考慮して、現在の論理的なグリッド位置を割り出す
            int logX = (physX - _gridOffsetX + gridSize) % gridSize;
            int logZ = (physZ - _gridOffsetZ + gridSize) % gridSize;

            // グリッド中央のインデックスを求める
            int centerOffset = gridSize / 2;

            float newX = (px + logX - centerOffset) * tileSize;
            float newZ = (pz + logZ - centerOffset) * tileSize;

            panel.OnWarped(new Vector3(newX, panel.WorldCenter.y, newZ), tileSize);
        }

    }
}
