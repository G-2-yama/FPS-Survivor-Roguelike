using BalanceSim;

namespace BalanceSim.Tests;

public class TerrainTests
{
    private static Terrain SingleBlockedCell(int blockedX, int blockedZ)
    {
        const int cells = 4;
        var blocked = new bool[cells * cells];
        blocked[blockedZ * cells + blockedX] = true;
        return new Terrain(new SimTerrainDef
        {
            originX = 0f,
            originZ = 0f,
            tileSize = 2f,
            gridSize = 2,
            cellSize = 1f,
            cells = cells,
            blockedBits = Terrain.Encode(blocked),
        });
    }

    [Fact]
    public void IsBlocked_WrapsAroundPeriod()
    {
        Terrain terrain = SingleBlockedCell(1, 2);

        Assert.True(terrain.IsBlocked(new Vec2(1.5f, 2.5f)));
        Assert.True(terrain.IsBlocked(new Vec2(1.5f + 4f, 2.5f - 8f)));
        Assert.True(terrain.IsBlocked(new Vec2(1.5f - 4f, 2.5f + 4f)));
        Assert.False(terrain.IsBlocked(new Vec2(0.5f, 2.5f)));
    }

    [Fact]
    public void Move_StopsAtWallAndClearsBlockedVelocityAxis()
    {
        Terrain terrain = SingleBlockedCell(2, 0);
        var velocity = new Vec2(5f, 1f);

        Vec2 moved = terrain.Move(new Vec2(1.5f, 0.2f), new Vec2(1f, 0.2f), ref velocity);

        Assert.True(moved.X < 2f);
        Assert.Equal(0.4f, moved.Z, 3);
        Assert.Equal(0f, velocity.X);
        Assert.Equal(1f, velocity.Z);
    }

    [Fact]
    public void WorldToGrid_UsesHalfTileOffsetLikeStageManager()
    {
        Terrain terrain = SingleBlockedCell(0, 0);

        Assert.Equal(0, terrain.WorldToGrid(0.99f));
        Assert.Equal(1, terrain.WorldToGrid(1f));
        Assert.Equal(-1, terrain.WorldToGrid(-1.01f));
    }
}
