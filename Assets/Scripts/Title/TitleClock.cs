using UnityEngine;

public static class TitleClock
{
    private const float MaxStep = 0.1f;

    private static int lastFrame = -1;
    private static float now;

    public static float Now
    {
        get
        {
            if (lastFrame != Time.frameCount)
            {
                lastFrame = Time.frameCount;
                now += Mathf.Min(Time.unscaledDeltaTime, MaxStep);
            }

            return now;
        }
    }
}
