using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public readonly struct LevelChange
{
    public string Label { get; }
    public string Before { get; }
    public string After { get; }
    public string Unit { get; }

    public LevelChange(string label, string before, string after, string unit = "")
    {
        Label = label;
        Before = before ?? "";
        After = after ?? "";
        Unit = unit ?? "";
    }

    public static LevelChange Switch(string label, bool enabled)
    {
        return new LevelChange(label, "", enabled ? "有効化" : "無効化");
    }

    public static LevelChange LabelOnly(string label)
    {
        return new LevelChange(label, "", "");
    }

    public static void AddIfChanged(List<LevelChange> changes, string label, int current, int next, string unit = "")
    {
        if (current != next)
            changes.Add(new LevelChange(label, current.ToString(CultureInfo.InvariantCulture), next.ToString(CultureInfo.InvariantCulture), unit));
    }

    public static void AddIfChanged(List<LevelChange> changes, string label, float current, float next, string unit = "")
    {
        if (Mathf.Approximately(current, next))
            return;

        string before = current.ToString("0.###", CultureInfo.InvariantCulture);
        string after = next.ToString("0.###", CultureInfo.InvariantCulture);
        // 小さな差でも、丸めによって同じ値に見えないようにする。
        if (before == after)
        {
            before = current.ToString("G9", CultureInfo.InvariantCulture);
            after = next.ToString("G9", CultureInfo.InvariantCulture);
        }
        changes.Add(new LevelChange(label, before, after, unit));
    }

    public static string Signed(float value)
    {
        return value.ToString("+0.###;-0.###;0", CultureInfo.InvariantCulture);
    }

    public static string SignedPercent(float ratio)
    {
        return Signed(ratio * 100f) + "%";
    }

    public override string ToString()
    {
        if (Before.Length == 0 && After.Length == 0)
            return Label;
        if (Before.Length == 0)
            return $"{Label}: {After}";
        return $"{Label}: {Before}{Unit} → {After}{Unit}";
    }
}
