using UnityEngine;

public static class TitleEase
{
    public static float Out(float t) => CubicBezier(0.2f, 0.7f, 0.2f, 1f, t);

    public static float Ease(float t) => CubicBezier(0.25f, 0.1f, 0.25f, 1f, t);

    public static float InOut(float t) => 0.5f - Mathf.Cos(Mathf.PI * Mathf.Clamp01(t)) / 2f;

    private static float CubicBezier(float x1, float y1, float x2, float y2, float x)
    {
        x = Mathf.Clamp01(x);
        float t = x;
        for (int i = 0; i < 8; i++)
        {
            float error = Sample(x1, x2, t) - x;
            float slope = SampleSlope(x1, x2, t);
            if (Mathf.Abs(error) < 1e-4f || Mathf.Abs(slope) < 1e-6f)
                break;

            t = Mathf.Clamp01(t - error / slope);
        }

        return Sample(y1, y2, t);
    }

    private static float Sample(float p1, float p2, float t) =>
        ((1f - 3f * p2 + 3f * p1) * t + (3f * p2 - 6f * p1)) * t * t + 3f * p1 * t;

    private static float SampleSlope(float p1, float p2, float t) =>
        3f * (1f - 3f * p2 + 3f * p1) * t * t + 2f * (3f * p2 - 6f * p1) * t + 3f * p1;
}
