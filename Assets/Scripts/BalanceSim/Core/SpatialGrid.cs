using System;
using System.Collections.Generic;

namespace BalanceSim
{
    public sealed class SpatialGrid
    {
        private const int SlotBits = 6;
        private const int SlotMask = (1 << SlotBits) - 1;

        private readonly float _cellSize;
        private readonly List<EnemyAgent>[] _slots = new List<EnemyAgent>[1 << (SlotBits * 2)];
        private readonly long[] _slotKeys = new long[1 << (SlotBits * 2)];
        private readonly List<int> _usedSlots = new();
        private readonly Dictionary<long, List<EnemyAgent>> _overflow = new();
        private readonly Stack<List<EnemyAgent>> _spare = new();

        public SpatialGrid(float cellSize)
        {
            _cellSize = cellSize;
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = new List<EnemyAgent>();
            }
        }

        public void Rebuild(List<EnemyAgent> enemies)
        {
            foreach (int slot in _usedSlots)
            {
                _slots[slot].Clear();
            }
            _usedSlots.Clear();

            foreach (List<EnemyAgent> list in _overflow.Values)
            {
                list.Clear();
                _spare.Push(list);
            }
            _overflow.Clear();

            foreach (EnemyAgent enemy in enemies)
            {
                if (enemy.Presence != Presence.Active)
                {
                    continue;
                }

                int x = Cell(enemy.Position.X);
                int z = Cell(enemy.Position.Z);
                long key = Key(x, z);
                int slot = Slot(x, z);
                List<EnemyAgent> slotList = _slots[slot];
                if (slotList.Count == 0)
                {
                    _slotKeys[slot] = key;
                    _usedSlots.Add(slot);
                    slotList.Add(enemy);
                }
                else if (_slotKeys[slot] == key)
                {
                    slotList.Add(enemy);
                }
                else
                {
                    if (!_overflow.TryGetValue(key, out List<EnemyAgent> list))
                    {
                        list = _spare.Count > 0 ? _spare.Pop() : new List<EnemyAgent>();
                        _overflow[key] = list;
                    }
                    list.Add(enemy);
                }
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
                    long key = Key(x, z);
                    int slot = Slot(x, z);
                    if (_slots[slot].Count > 0 && _slotKeys[slot] == key)
                    {
                        results.AddRange(_slots[slot]);
                    }
                    else if (_overflow.Count > 0 && _overflow.TryGetValue(key, out List<EnemyAgent> list))
                    {
                        results.AddRange(list);
                    }
                }
            }
        }

        private int Cell(float coord) => (int)MathF.Floor(coord / _cellSize);

        private static int Slot(int x, int z) => ((z & SlotMask) << SlotBits) | (x & SlotMask);

        private static long Key(int x, int z) => ((long)x << 32) | (uint)z;
    }
}
