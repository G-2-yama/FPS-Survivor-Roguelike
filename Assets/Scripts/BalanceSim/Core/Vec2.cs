using System;

namespace BalanceSim
{
    public readonly struct Vec2
    {
        public readonly float X;
        public readonly float Z;

        public Vec2(float x, float z)
        {
            X = x;
            Z = z;
        }

        public static Vec2 Zero => new(0f, 0f);
        public static Vec2 Forward => new(0f, 1f);

        public float SqrLength => X * X + Z * Z;
        public float Length => MathF.Sqrt(SqrLength);

        public Vec2 Normalized
        {
            get
            {
                float length = Length;
                return length > 1e-5f ? new Vec2(X / length, Z / length) : Zero;
            }
        }

        public Vec2 Right => new(Z, -X);

        public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Z + b.Z);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Z - b.Z);
        public static Vec2 operator -(Vec2 a) => new(-a.X, -a.Z);
        public static Vec2 operator *(Vec2 a, float s) => new(a.X * s, a.Z * s);

        public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Z * b.Z;

        public Vec2 RotateDegrees(float degrees)
        {
            float rad = degrees * (MathF.PI / 180f);
            float cos = MathF.Cos(rad);
            float sin = MathF.Sin(rad);
            return new Vec2(X * cos + Z * sin, -X * sin + Z * cos);
        }

        public Vec2 LocalToWorld(float localX, float localZ) => Right * localX + this * localZ;
    }
}
