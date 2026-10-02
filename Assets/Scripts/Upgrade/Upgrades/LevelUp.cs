using System.Collections.Generic;
using System.Globalization;
using UnityEngine;


[CreateAssetMenu(menuName = "Upgrade/LevelUp")]
public class LevelUp : UpgradeBase
{
    [SerializeField] private SlotType targetType;
    public override bool IsAvailable()
    {
        if (!player.Inventory.HasWeapon(targetType))
            return false;

        WeaponData currentData = player.Inventory.GetWeaponData(targetType);
        WeaponData nextData = currentData.NextLevelData;
        if (nextData == null || nextData.IsEmpty)
            return false;

        UpdateDescription(currentData, nextData);
        return true;
    }

    public override void Apply()
    {
        Weapon target = player.Inventory.WeaponSlots[targetType];
        target.LevelUp();
    }

    /// <summary>
    /// 武器のレベルアップの説明更新
    /// </summary>
    private void UpdateDescription(WeaponData currentData, WeaponData nextData)
    {
        icon = nextData.Icon;
        displayName = nextData.DisplayName + " 強化";
        description = BuildDescription(currentData, nextData);
    }

    /// <summary>
    /// 武器の基礎ステータスを比較し、変化した項目のみを表示する。
    /// 時間短縮や反動軽減、強化に伴う低下も変更前後の値で確認できる。
    /// </summary>
    private string BuildDescription(WeaponData current, WeaponData next)
    {
        var changes = new List<string>();
        AddChange(changes, "ダメージ", current.Damage, next.Damage);
        AddChange(changes, "装弾数", current.MagazineSize, next.MagazineSize);
        AddChange(changes, "リロード時間", current.ReloadTime, next.ReloadTime, "秒");
        AddChange(changes, "射撃間隔", current.FireInterval, next.FireInterval, "秒");
        AddChange(changes, "バースト数", current.BurstCount, next.BurstCount);
        // 単発射撃ではバースト間隔は使われない。
        if (current.BurstCount > 1 || next.BurstCount > 1)
            AddChange(changes, "バースト間隔", current.BurstInterval, next.BurstInterval, "秒");
        AddChange(changes, "チャージ時間", current.ChargeTime, next.ChargeTime, "秒");
        AddChange(changes, "ノックバック", current.KnockbackForce, next.KnockbackForce);
        AddChange(changes, "拡散角度", current.SpreadAngle, next.SpreadAngle, "度");
        AddChange(changes, "縦反動", current.Recoil.PitchKick, next.Recoil.PitchKick);
        AddChange(changes, "横反動", current.Recoil.YawKick, next.Recoil.YawKick);
        AddChange(changes, "横反動のばらつき", current.Recoil.YawRandomness, next.Recoil.YawRandomness);
        AddChange(changes, "反動の復元力", current.Recoil.ReturnStrength, next.Recoil.ReturnStrength);
        AddChange(changes, "反動の減衰", current.Recoil.Damping, next.Recoil.Damping);
        AddChange(changes, "縦反動の上限", current.Recoil.MaxPitch, next.Recoil.MaxPitch);

        if (current.AutoFire != next.AutoFire)
            changes.Add("自動射撃: " + (next.AutoFire ? "有効化" : "無効化"));
        if (current.AutoReload != next.AutoReload)
            changes.Add("自動リロード: " + (next.AutoReload ? "有効化" : "無効化"));
        if (current.TriggerType != next.TriggerType)
            changes.Add("射撃方式: " + (next.TriggerType == WeaponTriggerType.FullAuto ? "フルオート" : "セミオート"));
        if (current.FireModeData != next.FireModeData)
            changes.Add("攻撃パターン変更");

        return changes.Count > 0 ? string.Join("\n", changes) : "基礎ステータスの変化なし";
    }

    private void AddChange(List<string> changes, string label, int current, int next)
    {
        if (current != next)
            changes.Add($"{label}: {current} → {next}");
    }

    private void AddChange(List<string> changes, string label, float current, float next, string unit = "")
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
        changes.Add($"{label}: {before}{unit} → {after}{unit}");
    }
}
