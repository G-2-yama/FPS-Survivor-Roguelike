using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// WeaponDataのバランス調整用EditorWindow。
/// ゲーム本体のデータには影響を与えず、Editor上でDPS調整を行う。
/// </summary>
public class WeaponBalanceWindow : EditorWindow
{
    /// <summary>
    /// Editor専用の調整値。
    /// WeaponDataには保存せずEditorPrefsで管理する。
    /// </summary>
    private class BalanceInfo
    {
        /// <summary>
        /// 1サイクル中のヒット回数。
        /// 例:
        /// ライフル30発 → 30
        /// ショットガン8ペレット×6発 → 48
        /// </summary>
        public int CycleHitCount = 1;

        /// <summary>
        /// 攻撃全体の命中率。
        /// 0～1で管理する。
        /// </summary>
        public float HitRate = 1f;

        /// <summary>
        /// 同時に攻撃できる敵数。
        /// 範囲攻撃などで使用する。
        /// </summary>
        public int EnemyCount = 1;
    }


    private readonly List<WeaponData> _weapons = new();
    private readonly Dictionary<WeaponData, BalanceInfo> _balanceInfos = new();

    private Vector2 _scrollPosition;


    // 表示幅
    private const float DataWidth = 100f;
    private const float NameWidth = 150f;
    private const float DamageWidth = 60f;
    private const float KnockbackWidth = 90f;
    private const float NormalDpsWidth = 90f;
    private const float CycleTimeWidth = 90f;
    private const float HitRateWidth = 100f;
    private const float EnemyWidth = 70f;
    private const float EffectiveDpsWidth = 110f;


    /// <summary>
    /// Weapon Balance Windowを開く。
    /// </summary>
    [MenuItem("Tools/Weapon Balance Viewer")]
    private static void Open()
    {
        GetWindow<WeaponBalanceWindow>("Weapon Balance");
    }


    /// <summary>
    /// Window生成時にWeaponDataを取得する。
    /// </summary>
    private void OnEnable()
    {
        ReloadWeapons();
    }


    /// <summary>
    /// プロジェクト内のWeaponDataを取得する。
    /// </summary>
    private void ReloadWeapons()
    {
        _weapons.Clear();
        _balanceInfos.Clear();

        string[] guids = AssetDatabase.FindAssets("t:WeaponData");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(path);

            if (weapon == null || weapon.IsEmpty)
            {
                continue;
            }

            _weapons.Add(weapon);
            _balanceInfos.Add(weapon, LoadBalanceInfo(weapon));
        }

        _weapons.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
    }


    /// <summary>
    /// WeaponData固有の保存キー取得用GUIDを取得する。
    /// </summary>
    private string GetGuid(WeaponData weapon)
    {
        return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(weapon));
    }


    /// <summary>
    /// EditorPrefsから調整値を読み込む。
    /// </summary>
    private BalanceInfo LoadBalanceInfo(WeaponData weapon)
    {
        string guid = GetGuid(weapon);

        return new BalanceInfo
        {
            CycleHitCount = EditorPrefs.GetInt($"{guid}_CycleHitCount", 1),
            HitRate = EditorPrefs.GetFloat($"{guid}_HitRate", 1f),
            EnemyCount = EditorPrefs.GetInt($"{guid}_EnemyCount", 1)
        };
    }


    /// <summary>
    /// 調整値をEditorPrefsへ保存する。
    /// </summary>
    private void SaveBalanceInfo(WeaponData weapon, BalanceInfo info)
    {
        string guid = GetGuid(weapon);

        EditorPrefs.SetInt($"{guid}_CycleHitCount", info.CycleHitCount);
        EditorPrefs.SetFloat($"{guid}_HitRate", info.HitRate);
        EditorPrefs.SetInt($"{guid}_EnemyCount", info.EnemyCount);
    }
        /// <summary>
    /// Window描画処理。
    /// </summary>
    private void OnGUI()
    {
        if (GUILayout.Button("Reload"))
        {
            ReloadWeapons();
        }

        GUILayout.Space(5);

        DrawHeader();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        foreach (WeaponData weapon in _weapons)
        {
            DrawRow(weapon);
        }

        EditorGUILayout.EndScrollView();
    }


    /// <summary>
    /// 表のヘッダーを描画する。
    /// </summary>
    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        GUILayout.Label("Data", GUILayout.Width(DataWidth));
        GUILayout.Label("Name", GUILayout.Width(NameWidth));
        GUILayout.Label("Damage", GUILayout.Width(DamageWidth));
        GUILayout.Label("Knockback", GUILayout.Width(KnockbackWidth));
        GUILayout.Label("Normal DPS", GUILayout.Width(NormalDpsWidth));
        GUILayout.Label("Cycle Time", GUILayout.Width(CycleTimeWidth));
        GUILayout.Label("Hit Rate", GUILayout.Width(HitRateWidth));
        GUILayout.Label("Enemy", GUILayout.Width(EnemyWidth));
        GUILayout.Label("Effective DPS", GUILayout.Width(EffectiveDpsWidth));

        EditorGUILayout.EndHorizontal();
    }


    /// <summary>
    /// 武器1行分を描画する。
    /// </summary>
    private void DrawRow(WeaponData weapon)
    {
        BalanceInfo info = _balanceInfos[weapon];

        EditorGUILayout.BeginHorizontal();

        // ScriptableObject名
        if (GUILayout.Button(weapon.name, GUILayout.Width(DataWidth)))
        {
            Selection.activeObject = weapon;
            EditorGUIUtility.PingObject(weapon);
        }

        // 表示名
        GUILayout.Label(weapon.DisplayName, GUILayout.Width(NameWidth));

        // 基本ステータス
        GUILayout.Label(weapon.Damage.ToString(), GUILayout.Width(DamageWidth));
        GUILayout.Label(weapon.KnockbackForce.ToString("F1"), GUILayout.Width(KnockbackWidth));

        // 通常DPS
        GUILayout.Label(CalculateNormalDPS(weapon).ToString("F1"), GUILayout.Width(NormalDpsWidth));

        // 1サイクル時間
        GUILayout.Label(CalculateCycleTime(weapon).ToString("F2"), GUILayout.Width(CycleTimeWidth));

        info.HitRate = EditorGUILayout.Slider(
            info.HitRate,
            0f,
            1f,
            GUILayout.Width(HitRateWidth));

        info.EnemyCount = Mathf.Max(
            1,
            EditorGUILayout.IntField(info.EnemyCount, GUILayout.Width(EnemyWidth)));

        // 実戦想定DPS
        GUILayout.Label(
            CalculateEffectiveDPS(weapon, info).ToString("F1"),
            GUILayout.Width(EffectiveDpsWidth));


        SaveBalanceInfo(weapon, info);

        EditorGUILayout.EndHorizontal();
    }


    /// <summary>
    /// 通常DPSを計算する。
    /// 命中率や敵数補正は含まない。
    /// </summary>
    private float CalculateNormalDPS(WeaponData weapon)
    {
        float cycleTime = CalculateCycleTime(weapon);

        if (cycleTime <= 0f)
        {
            return 0f;
        }

        float cycleDamage = weapon.Damage * weapon.MagazineSize;

        return cycleDamage / cycleTime;
    }



    /// <summary>
    /// 1マガジン撃ち切りからリロード完了までの時間を計算する。
    /// </summary>
    private float CalculateCycleTime(WeaponData weapon)
    {
        float fireTime =(weapon.MagazineSize - 1) * weapon.BurstInterval;

        return weapon.ChargeTime + fireTime + weapon.FireInterval +weapon.ReloadTime;
    }


    /// <summary>
    /// 命中率などを考慮した1サイクルダメージを計算する。
    /// </summary>
    private float CalculateCycleDamage(WeaponData weapon, BalanceInfo info)
    {
        return weapon.Damage * weapon.MagazineSize * 
            info.CycleHitCount * info.HitRate * info.EnemyCount;
    }


    /// <summary>
    /// 実戦を想定したDPSを計算する。
    /// </summary>
    private float CalculateEffectiveDPS(WeaponData weapon, BalanceInfo info)
    {
        float cycleTime = CalculateCycleTime(weapon);

        if (cycleTime <= 0f)
        {
            return 0f;
        }

        return CalculateCycleDamage(weapon, info) / cycleTime;
    }
}