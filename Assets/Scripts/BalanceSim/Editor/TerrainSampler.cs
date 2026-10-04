using System.Collections.Generic;
using System.Linq;
using InfiniteTileWorld;
using UnityEngine;

namespace BalanceSim.Editor
{
    public static class TerrainSampler
    {
        public sealed class Cache
        {
            internal SimTerrainDef def;
            internal float bottom;
            internal float top;
            internal float blockedRatio;

            internal bool Matches(float originX, float originZ, float tileSize, int gridSize, int cells, float bottom, float top)
            {
                return def != null && def.originX == originX && def.originZ == originZ && def.tileSize == tileSize && def.gridSize == gridSize
                    && def.cells == cells && this.bottom == bottom && this.top == top;
            }
        }

        public static SimTerrainDef Sample(StageManager stage, Player player, float requestedCellSize, Cache cache, List<string> warnings, out float blockedRatio)
        {
            float tileSize = stage.TileSize;
            int gridSize = stage.GridSize;
            List<StagePanel> panels = stage.Panels.Where(p => p != null).ToList();
            if (panels.Count != gridSize * gridSize)
            {
                warnings.Add($"StageManager のパネル数 {panels.Count} が gridSize²（{gridSize * gridSize}）と合いません");
            }

            float period = tileSize * gridSize;
            int cells = Mathf.Max(1, Mathf.RoundToInt(period / requestedCellSize));
            float cellSize = period / cells;
            float originX = panels.Count > 0 ? panels.Min(p => p.transform.position.x) - tileSize * 0.5f : 0f;
            float originZ = panels.Count > 0 ? panels.Min(p => p.transform.position.z) - tileSize * 0.5f : 0f;

            Physics.SyncTransforms();
            var controller = player.GetComponent<CharacterController>();
            float height = controller != null ? controller.height : 2f;
            float stepOffset = controller != null ? controller.stepOffset : 0.3f;
            Vector3 playerPosition = player.transform.position;
            float groundY = playerPosition.y - height * 0.5f;
            if (Physics.Raycast(playerPosition, Vector3.down, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
            {
                groundY = hit.point.y;
            }
            else
            {
                warnings.Add("プレイヤーの足元に地面が見つかりません。プレイヤーの高さから地面を推定します");
            }

            float bottom = groundY + stepOffset;
            float top = groundY + height;
            if (cache.Matches(originX, originZ, tileSize, gridSize, cells, bottom, top))
            {
                blockedRatio = cache.blockedRatio;
                WarnIfStartBlocked(cache.def, playerPosition, warnings);
                return cache.def;
            }

            var halfExtents = new Vector3(cellSize * 0.5f, (top - bottom) * 0.5f, cellSize * 0.5f);
            float centerY = (top + bottom) * 0.5f;

            var blocked = new bool[cells * cells];
            var buffer = new Collider[32];
            int blockedCount = 0;
            for (int z = 0; z < cells; z++)
            {
                for (int x = 0; x < cells; x++)
                {
                    var center = new Vector3(originX + (x + 0.5f) * cellSize, centerY, originZ + (z + 0.5f) * cellSize);
                    int count = Physics.OverlapBoxNonAlloc(center, halfExtents, buffer, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);
                    for (int i = 0; i < count; i++)
                    {
                        if (buffer[i].attachedRigidbody == null)
                        {
                            blocked[z * cells + x] = true;
                            blockedCount++;
                            break;
                        }
                    }
                }
            }

            blockedRatio = (float)blockedCount / blocked.Length;
            if (blockedCount == 0)
            {
                warnings.Add("通れないマスが1つもありません。地形を読めていない可能性があります");
            }

            var def = new SimTerrainDef
            {
                originX = originX,
                originZ = originZ,
                tileSize = tileSize,
                gridSize = gridSize,
                cellSize = cellSize,
                cells = cells,
                blockedBits = Terrain.Encode(blocked),
            };

            cache.def = def;
            cache.bottom = bottom;
            cache.top = top;
            cache.blockedRatio = blockedRatio;
            WarnIfStartBlocked(def, playerPosition, warnings);
            return def;
        }

        private static void WarnIfStartBlocked(SimTerrainDef def, Vector3 playerPosition, List<string> warnings)
        {
            if (new Terrain(def).IsBlocked(new Vec2(playerPosition.x, playerPosition.z)))
            {
                warnings.Add("プレイヤーの初期位置が通れないマスになっています");
            }
        }
    }
}
