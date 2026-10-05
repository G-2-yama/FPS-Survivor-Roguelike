using System;

namespace BalanceSim
{
    public sealed class Terrain
    {
        private readonly float _originX;
        private readonly float _originZ;
        private readonly float _cellSize;
        private readonly int _cells;
        private readonly bool[] _blocked;

        public float TileSize { get; }
        public int GridSize { get; }
        public float Period => TileSize * GridSize;
        public float CellSize => _cellSize;

        public Terrain(SimTerrainDef def)
        {
            TileSize = def.tileSize;
            GridSize = def.gridSize;
            _originX = def.originX;
            _originZ = def.originZ;
            _cellSize = def.cellSize;
            _cells = def.cells;
            _blocked = Decode(def.blockedBits, def.cells);
        }

        public static string Encode(bool[] blocked)
        {
            var bytes = new byte[(blocked.Length + 7) / 8];
            for (int i = 0; i < blocked.Length; i++)
            {
                if (blocked[i])
                {
                    bytes[i >> 3] |= (byte)(1 << (i & 7));
                }
            }
            return Convert.ToBase64String(bytes);
        }

        private static bool[] Decode(string bits, int cells)
        {
            if (string.IsNullOrEmpty(bits) || cells <= 0)
            {
                return null;
            }

            byte[] bytes = Convert.FromBase64String(bits);
            var blocked = new bool[cells * cells];
            for (int i = 0; i < blocked.Length; i++)
            {
                blocked[i] = (bytes[i >> 3] & (1 << (i & 7))) != 0;
            }
            return blocked;
        }

        public bool IsBlocked(Vec2 p)
        {
            if (_blocked == null)
            {
                return false;
            }

            int cx = CellIndex(p.X - _originX);
            int cz = CellIndex(p.Z - _originZ);
            return _blocked[cz * _cells + cx];
        }

        private int CellIndex(float offset)
        {
            int index = (int)MathF.Floor(offset / _cellSize) % _cells;
            return index < 0 ? index + _cells : index;
        }

        public int WorldToGrid(float coord) => (int)MathF.Floor((coord + TileSize * 0.5f) / TileSize);

        public float FreeDistance(Vec2 from, Vec2 dir, float maxDistance)
        {
            for (float d = _cellSize; d <= maxDistance; d += _cellSize)
            {
                if (IsBlocked(from + dir * d))
                {
                    return d - _cellSize;
                }
            }
            return maxDistance;
        }

        public Vec2 Move(Vec2 from, Vec2 delta, ref Vec2 velocity)
        {
            int steps = Math.Max(1, (int)MathF.Ceiling(delta.Length / _cellSize));
            Vec2 step = delta * (1f / steps);
            Vec2 p = from;
            bool blockedX = false;
            bool blockedZ = false;
            for (int i = 0; i < steps; i++)
            {
                if (!blockedX)
                {
                    var nx = new Vec2(p.X + step.X, p.Z);
                    if (IsBlocked(nx))
                    {
                        blockedX = true;
                    }
                    else
                    {
                        p = nx;
                    }
                }

                if (!blockedZ)
                {
                    var nz = new Vec2(p.X, p.Z + step.Z);
                    if (IsBlocked(nz))
                    {
                        blockedZ = true;
                    }
                    else
                    {
                        p = nz;
                    }
                }
            }

            if (blockedX || blockedZ)
            {
                velocity = new Vec2(blockedX ? 0f : velocity.X, blockedZ ? 0f : velocity.Z);
            }
            return p;
        }
    }
}
