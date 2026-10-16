using System.Collections.Generic;
using System.Globalization;

public readonly struct SlotTooltipContent
{
    public string Title { get; }
    public string Description { get; }
    public string Stats { get; }

    public SlotTooltipContent(string title, string description, string stats)
    {
        Title = title;
        Description = description;
        Stats = stats;
    }

    public static SlotTooltipContent For(Weapon weapon)
    {
        var lines = new List<string>
        {
            Line("ダメージ", weapon.GetDamageAmount().ToString(CultureInfo.InvariantCulture)),
            Line("装弾数", weapon.MagazineSize.ToString(CultureInfo.InvariantCulture)),
            Line("リロード時間", Number(weapon.ReloadTime), "秒"),
            Line("射撃間隔", Number(weapon.FireInterval), "秒"),
        };
        if (weapon.BurstCount > 1)
        {
            lines.Add(Line("バースト数", weapon.BurstCount.ToString(CultureInfo.InvariantCulture)));
            lines.Add(Line("バースト間隔", Number(weapon.BurstInterval), "秒"));
        }
        if (weapon.ChargeTime > 0f)
            lines.Add(Line("チャージ時間", Number(weapon.ChargeTime), "秒"));
        lines.Add(Line("ノックバック", Number(weapon.GetKnockbackForce())));
        lines.Add(Line("拡散角度", Number(weapon.SpreadAngle), "度"));
        lines.Add(Line("射撃方式", weapon.TriggerType == WeaponTriggerType.FullAuto ? "フルオート" : "セミオート"));

        return new SlotTooltipContent(weapon.DisplayName, weapon.Description, string.Join("\n", lines));
    }

    public static SlotTooltipContent For(Item item)
    {
        return new SlotTooltipContent(item.DisplayName, item.Description, "");
    }

    private static string Line(string label, string value, string unit = "") => $"{label}: {value}{unit}";

    private static string Number(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
