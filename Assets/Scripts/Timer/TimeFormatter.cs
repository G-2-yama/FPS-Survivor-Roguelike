using UnityEngine;

public static class TimeFormatter
{
    public static int ToTenths(float seconds)
    {
        return Mathf.Max(0, Mathf.RoundToInt(seconds * 10f));
    }

    public static string Format(float seconds, TimeDisplayFormat format)
    {
        return FormatTenths(ToTenths(seconds), format);
    }

    public static string FormatTenths(int tenths, TimeDisplayFormat format)
    {
        int wholeSeconds = tenths / 10;
        int fraction = tenths % 10;

        return format switch
        {
            TimeDisplayFormat.Seconds => $"{wholeSeconds}.{fraction}",
            _ => $"{wholeSeconds / 60:00}:{wholeSeconds % 60:00}.{fraction}",
        };
    }
}
