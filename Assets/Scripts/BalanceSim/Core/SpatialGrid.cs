using System;
using System.Collections.Generic;

namespace BalanceSim
{
    public sealed class SpatialGrid
    {
        private readonly float _cellSize;
        private readonly Dictionary<long, List<EnemyAgent>> _cells = new();
        private readonly Stack<List<EnemyAgent>> _spare = new();

        public SpatialGrid(float cellSize)
        {
            _cellSize = cellSize;
        }

        public void Rebuild(List<EnemyAgent> enemies)
        {
            foreach (List<EnemyAgent> list in _cells.Values)
            {
                list.Clear();
                _spare.Push(list);
            }
            _cells.Clear();

            foreach (EnemyAgent enemy in enemies)
            {
                if (enemy.Presence != Presence.Active)
                {
                    continue;
                }

                long key = Key(Cell(enemy.Position.X), Cell(enemy.Position.Z));
                if (!_cells.TryGetValue(key, out List<EnemyAgent> list))
                {
                    list = _spare.Count > 0 ? _spare.Pop() : new List<EnemyAgent>();
                    _cells[key] = list;
                }
                list.Add(enemy);
            }
        }

        public void Query(Vec2 center, float radius, List<EnemyAgent> results)
        {
            results.Clear();
            int minX = Cell(center.X - radius);
            int maxX = Cell(center.X + radius);
            int minZ = Cell(center.Z - radius);
            int maxZ = Cell(center.Z + radius);
            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    if (_cells.TryGetValue(Key(x, z), out List<EnemyAgent> list))
                    {
                        results.AddRange(list);
                    }
                }
            }
        }

        private int Cell(float coord) => (int)MathF.Floor(coord / _cellSize);

        private static long Key(int x, int z) => ((long)x << 32) | (uint)z;
    }
}
