namespace BalanceSim
{
    public static class Movement
    {
        public const float Gravity = 9.81f;

        public static void Apply(Body body, SimMovementDef move, Vec2 target, Vec2 targetFacing, float dt, Terrain terrain)
        {
            Vec2 toTarget = target - body.Position;
            switch (move.kind)
            {
                case SimMovementKind.Toward:
                {
                    if (toTarget.SqrLength <= 0.0001f)
                    {
                        return;
                    }

                    if (body.Velocity.Length > move.maxSpeed)
                    {
                        body.Velocity = body.Velocity.Normalized * move.maxSpeed;
                    }

                    Vec2 direction = toTarget.Normalized;
                    body.Velocity += direction * (move.acceleration * dt);
                    body.Facing = -direction;
                    break;
                }
                case SimMovementKind.Roll:
                {
                    if (toTarget.SqrLength <= 0.001f)
                    {
                        return;
                    }

                    body.Velocity += toTarget.Normalized * (move.moveForce * 0.7f * dt);
                    break;
                }
                case SimMovementKind.Jump:
                {
                    if (body.VerticalVelocity > 0.1f)
                    {
                        return;
                    }

                    body.Velocity += toTarget.Normalized * move.forwardForce;
                    body.VerticalVelocity += move.upwardForce;
                    break;
                }
                case SimMovementKind.Item:
                {
                    if (toTarget.SqrLength <= 0.0001f)
                    {
                        return;
                    }

                    Vec2 direction = toTarget.Normalized;
                    MovePosition(body, body.Position + direction * (move.chaseSpeed * dt), terrain);
                    body.Facing = -direction;
                    break;
                }
                case SimMovementKind.Orbit:
                {
                    Vec2 offset = body.Position - target;
                    if (offset.SqrLength <= 0.0001f)
                    {
                        offset = -targetFacing * move.orbitRadius;
                    }

                    float radiusDiff = move.orbitRadius - offset.Length;
                    offset += offset.Normalized * (radiusDiff * 0.1f);
                    offset = offset.RotateDegrees(move.orbitAngularSpeed * dt);
                    MovePosition(body, target + offset, terrain);
                    if (toTarget.SqrLength > 0.0001f)
                    {
                        body.Facing = -toTarget.Normalized;
                    }
                    break;
                }
            }
        }

        private static void MovePosition(Body body, Vec2 destination, Terrain terrain)
        {
            Vec2 ignored = Vec2.Zero;
            body.Position = terrain.Move(body.Position, destination - body.Position, ref ignored);
        }

        public static void Integrate(Body body, float linearDamping, bool useGravity, float dt, Terrain terrain)
        {
            if (useGravity)
            {
                body.VerticalVelocity -= Gravity * dt;
                body.Height += body.VerticalVelocity * dt;
                if (body.Height <= 0f)
                {
                    body.Height = 0f;
                    if (body.VerticalVelocity < 0f)
                    {
                        body.VerticalVelocity = 0f;
                    }
                }
            }

            if (linearDamping > 0f)
            {
                body.Velocity *= 1f / (1f + linearDamping * dt);
            }

            if (body.Velocity.SqrLength > 0f)
            {
                body.Position = terrain.Move(body.Position, body.Velocity * dt, ref body.Velocity);
            }
        }
    }
}
