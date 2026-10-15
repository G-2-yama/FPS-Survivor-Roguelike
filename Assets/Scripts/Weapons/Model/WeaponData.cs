using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器ごとの差分パラメータを保持するデータ定義
/// </summary>
[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public virtual bool IsEmpty => false;

    [Header("Identity")]
    [Tooltip("装備の ID。同じ装備かどうかの判定に使う")]
    [SerializeField] private string weaponId;
    [Tooltip("タイトル画面の装備選択で大きく表示する画像 未設定でも動作します")]
    [SerializeField] private Sprite artwork;

    [Header("Classification")]
    [Tooltip("全レベル共通の武器の種類")]
    [SerializeField] private WeaponType weaponType = WeaponType.Main;

    [Header("Attributes")]
    [Tooltip("この装備が持つ属性")]
    [SerializeField] private List<AttributeDefinition> attributes = new List<AttributeDefinition>();
    [Tooltip("この装備の固有効果")]
    [SerializeField] private List<WeaponTrait> traits = new List<WeaponTrait>();

    [Header("Levels")]
    [Tooltip("レベル順の値。各要素はそのレベルの全部の値（差分ではない）")]
    [SerializeField] private List<WeaponLevelData> levels = new List<WeaponLevelData>();
    [Tooltip("最後のレベルを Evo として扱う")]
    [SerializeField] private bool lastLevelIsEvo = true;

    public string WeaponId => weaponId;
    public Sprite Artwork => artwork;
    public WeaponType WeaponType => weaponType;
    public IReadOnlyList<AttributeDefinition> Attributes => attributes;
    public IReadOnlyList<WeaponTrait> Traits => traits;

    public int LevelCount => levels.Count;
    public int MaxLevelIndex => Mathf.Max(0, levels.Count - 1);

    public WeaponLevelData GetLevel(int level)
    {
        return level >= 0 && level < levels.Count ? levels[level] : WeaponLevelData.Empty;
    }

    public bool IsEvoLevel(int level) => lastLevelIsEvo && level == MaxLevelIndex && LevelCount > 1;

    public string DisplayName => GetLevel(0).DisplayName;
    public string Description => GetLevel(0).Description;
    public Sprite Icon => GetLevel(0).Icon;

    /// <summary>
    /// 武器の基礎ステータスを比較し、変化した項目のみを返す。
    /// 時間短縮や反動軽減、強化に伴う低下も変更前後の値で確認できる。
    /// </summary>
    public IReadOnlyList<LevelChange> GetLevelChanges(int level)
    {
        var changes = new List<LevelChange>();
        if (level < 0 || level + 1 >= levels.Count)
            return changes;

        WeaponLevelData current = levels[level];
        WeaponLevelData next = levels[level + 1];

        LevelChange.AddIfChanged(changes, "ダメージ", current.Damage, next.Damage);
        LevelChange.AddIfChanged(changes, "装弾数", current.MagazineSize, next.MagazineSize);
        LevelChange.AddIfChanged(changes, "リロード時間", current.ReloadTime, next.ReloadTime, "秒");
        LevelChange.AddIfChanged(changes, "射撃間隔", current.FireInterval, next.FireInterval, "秒");
        LevelChange.AddIfChanged(changes, "バースト数", current.BurstCount, next.BurstCount);
        // 単発射撃ではバースト間隔は使われない。
        if (current.BurstCount > 1 || next.BurstCount > 1)
            LevelChange.AddIfChanged(changes, "バースト間隔", current.BurstInterval, next.BurstInterval, "秒");
        LevelChange.AddIfChanged(changes, "チャージ時間", current.ChargeTime, next.ChargeTime, "秒");
        LevelChange.AddIfChanged(changes, "ノックバック", current.KnockbackForce, next.KnockbackForce);
        LevelChange.AddIfChanged(changes, "拡散角度", current.SpreadAngle, next.SpreadAngle, "度");
        LevelChange.AddIfChanged(changes, "縦反動", current.Recoil.PitchKick, next.Recoil.PitchKick);
        LevelChange.AddIfChanged(changes, "横反動", current.Recoil.YawKick, next.Recoil.YawKick);
        LevelChange.AddIfChanged(changes, "横反動のばらつき", current.Recoil.YawRandomness, next.Recoil.YawRandomness);
        LevelChange.AddIfChanged(changes, "反動の復元力", current.Recoil.ReturnStrength, next.Recoil.ReturnStrength);
        LevelChange.AddIfChanged(changes, "反動の減衰", current.Recoil.Damping, next.Recoil.Damping);
        LevelChange.AddIfChanged(changes, "縦反動の上限", current.Recoil.MaxPitch, next.Recoil.MaxPitch);

        if (current.AutoFire != next.AutoFire)
            changes.Add(LevelChange.Switch("自動射撃", next.AutoFire));
        if (current.AutoReload != next.AutoReload)
            changes.Add(LevelChange.Switch("自動リロード", next.AutoReload));
        if (current.TriggerType != next.TriggerType)
            changes.Add(new LevelChange("射撃方式", "", next.TriggerType == WeaponTriggerType.FullAuto ? "フルオート" : "セミオート"));
        if (current.FireModeData != next.FireModeData)
            changes.Add(LevelChange.LabelOnly("攻撃パターン変更"));

        return changes;
    }
}

/// <summary>
/// 武器の見た目・演出に関するデータ
/// </summary>
[System.Serializable]
public class WeaponVisual
{
    [SerializeField] private GameObject weaponModelPrefab;
    public GameObject WeaponModelPrefab => weaponModelPrefab;
}

/// <summary>
/// 射撃トリガーの挙動に関するデータ
/// </summary>
[System.Serializable]
public class WeaponTriggerConfig
{
    [SerializeField] private bool autoFire = false;
    [SerializeField] private bool autoReload = false;
    [SerializeField] private WeaponTriggerType triggerType = WeaponTriggerType.FullAuto;

    public bool AutoFire => autoFire;
    public bool AutoReload => autoReload;
    public WeaponTriggerType TriggerType => triggerType;
}

public enum WeaponTriggerType
{
    SemiAuto = 0,
    FullAuto = 1,
}

public enum WeaponType
{
    Main = 0,
    Ability = 1,
    AutoWeapon = 2,
}

/// <summary>
/// 攻撃威力・精度
/// </summary>
[System.Serializable]
public sealed class DamageProfile
{
    public static DamageProfile Empty { get; } = new DamageProfile();

    [SerializeField] private int damage = 1;
    [SerializeField] private float knockbackForce = 1f;
    [SerializeField, Range(0f, 45f)] private float spreadAngle;

    public int Damage => damage;
    public float KnockbackForce => knockbackForce;
    public float SpreadAngle => spreadAngle;

    private DamageProfile() { }
}

/// <summary>
/// 射撃リズム
/// </summary>
[System.Serializable]
public sealed class FireRhythmProfile
{
    public static FireRhythmProfile Empty { get; } = new FireRhythmProfile();

    [SerializeField] private float fireInterval = 0.1f;
    [SerializeField] private int burstCount = 1;
    [SerializeField] private float burstInterval = 0.05f;
    [SerializeField] private float chargeTime = 0.0f;

    public float FireInterval => fireInterval;
    public int BurstCount => burstCount;
    public float BurstInterval =>   burstInterval;
    public float ChargeTime => chargeTime;

    private FireRhythmProfile() { }
}

/// <summary>
/// 反動
/// </summary>
[System.Serializable]
public sealed class Recoil
{
    public static Recoil Empty { get; } = new Recoil();

    /// <summary>
    /// Step 1回で進める時間の上限（秒）。
    /// damping × dt が2未満、かつ returnStrength × dt² + 2 × damping × dt が4未満でないと、
    /// 反動が振動しながら増えて NaN になる。
    /// </summary>
    private const float MaxStepTime = 1f / 60f;

    [SerializeField] private float pitchKick = 1.2f;
    [SerializeField] private float yawKick = 0.4f;
    [SerializeField] private float yawRandomness = 1f;
    [SerializeField] private float returnStrength = 20f;
    [SerializeField] private float damping = 18f;
    [SerializeField] private float maxPitch = 20f;

    public float PitchKick => pitchKick;
    public float YawKick => yawKick;
    public float YawRandomness => yawRandomness;
    public float ReturnStrength => returnStrength;
    public float Damping => damping;
    public float MaxPitch => maxPitch;

    private Vector2 recoilOffset = Vector2.zero;
    private Vector2 recoilVelocity = Vector2.zero;

    public Vector2 RecoilOffset => recoilOffset;

    /// <summary>
    /// 発射時に反動を加える
    /// </summary>
    public void AddRecoil(float recoilMultiplier = 1f)
    {
        float yaw = Random.Range(-YawKick, YawKick) * YawRandomness;

        recoilOffset.y += PitchKick * recoilMultiplier;
        recoilOffset.x += yaw * recoilMultiplier;
    }

    /// <summary>
    /// 毎フレーム更新
    /// </summary>
    public Vector2 Tick(float deltaTime)
    {
        int steps = Mathf.Max(1, Mathf.CeilToInt(deltaTime / MaxStepTime));
        float stepTime = deltaTime / steps;

        for (int i = 0; i < steps; i++)
        {
            Step(stepTime);
        }

        return recoilOffset;
    }

    public void Reset()
    {
        recoilOffset = Vector2.zero;
        recoilVelocity = Vector2.zero;
    }

    private void Step(float deltaTime)
    {
        Vector2 accel =
            (-ReturnStrength * recoilOffset) -
            (Damping * recoilVelocity);

        recoilVelocity += accel * deltaTime;
        recoilOffset += recoilVelocity * deltaTime;

        recoilOffset.y =
            Mathf.Clamp(recoilOffset.y, -MaxPitch, MaxPitch);
    }
}

/// <summary>
/// 弾薬・リロード
/// </summary>
[System.Serializable]
public sealed class AmmoProfile
{
    public static AmmoProfile Empty { get; } = new AmmoProfile();

    [SerializeField] private int magazineSize = 1;
    [SerializeField] private float reloadTime = 1.0f;

    public int MagazineSize => magazineSize;
    public float ReloadTime => reloadTime;

    private AmmoProfile() { }
}