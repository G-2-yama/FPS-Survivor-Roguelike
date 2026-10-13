using System;

/// <summary>プレイヤーごとのレベル、残余経験値、次の必要経験値を管理する</summary>
public sealed class PlayerProgression
{
    private readonly double rate;
    private double experience;
    private double requiredExperience;
    public float Experience => (float)experience;
    public float RequiredExperience => (float)Math.Min(float.MaxValue, requiredExperience);
    public int Level { get; private set; }

    /// <summary>成長設定からプレイヤー専用の初期状態を作る</summary>
    public PlayerProgression(PlayerProgressionSettings settings)
    {
        Level = settings.InitialLevel;
        requiredExperience = settings.LevelUpRequiredExp;
        rate = settings.LevelUpRate;
    }

    /// <summary>経験値を加算し、複数レベル分をまとめて処理して上昇数を返す</summary>
    public int AddExperience(float amount)
    {
        if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount)) return 0;
        experience += amount;
        int capacity = int.MaxValue - Level;
        if (experience < requiredExperience || capacity == 0) return 0;

        // 倍率1でも大量の経験値を1レベルずつ処理せず、等比数列の合計から上昇数を求める
        double estimate = rate == 1d
            ? Math.Floor(experience / requiredExperience)
            : Math.Floor(Math.Log(1d + experience * (rate - 1d) / requiredExperience) / Math.Log(rate));
        int count = (int)Math.Min(capacity, Math.Max(0d, estimate));
        double consumed = RequiredForLevels(count);
        // 浮動小数点の境界誤差でレベルを余分に進めたり、取りこぼしたりしないよう補正する
        while (count > 0 && consumed > experience) consumed = RequiredForLevels(--count);
        while (count < capacity && RequiredForLevels(count + 1) <= experience)
            consumed = RequiredForLevels(++count);
        experience = Math.Max(0d, experience - consumed);
        requiredExperience *= Math.Pow(rate, count);
        Level += count;
        return count;
    }

    /// <summary>現在のレベルから指定回数のレベルアップに必要な経験値合計を求める</summary>
    private double RequiredForLevels(int count) => rate == 1d
        ? requiredExperience * count
        : requiredExperience * (Math.Pow(rate, count) - 1d) / (rate - 1d);
}