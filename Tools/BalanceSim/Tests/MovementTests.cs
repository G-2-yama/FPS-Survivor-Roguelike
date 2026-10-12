using BalanceSim;

namespace BalanceSim.Tests;

public class MovementTests
{
    [Fact]
    public void RollStopsAcceleratingAtMaxSpeed()
    {
        var body = new EnemyAgent();
        var move = new SimMovementDef { kind = SimMovementKind.Roll, moveForce = 4f, maxSpeed = 8f };
        const float dt = 1f / 30f;
        for (int i = 0; i < 300; i++)
        {
            Movement.Apply(body, move, new Vec2(1000f, 0f), Vec2.Forward, dt, null);
        }

        Assert.InRange(body.Velocity.Length, 8f, 8f + move.moveForce * 0.7f * dt + 1e-4f);
    }
}
