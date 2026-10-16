using UnityEngine;

public static class EnemyStatScaling
{
    public static int Scale(int value, float multiplier) => Mathf.FloorToInt(value * multiplier + 0.5f);
}
