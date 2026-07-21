using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// WeaponDataのバランス調整用ウィンドウ
/// </summary>
public class WeaponBalanceWindow : EditorWindow
{
    /// <summary>
    /// Editor専用バランス調整データ
    /// </summary>
    private class BalanceInfo
    {
        public float HitRate = 1f;
        public int HitCount = 1;

        public float ExpectedDPS(WeaponData weapon)
        {
            return weapon.AverageDPS * HitRate * HitCount;
        }
    }

    private readonly List<WeaponData> _weapons = new();
    private readonly Dictionary<WeaponData, BalanceInfo> _balanceInfos = new();

    private Vector2 _scrollPosition;

    private const float IndexWidth = 100f;
    private const float NameWidth = 180f;
    private const float TypeWidth = 80f;
    private const float DamageWidth = 60f;
    private const float AverageDpsWidth = 80f;
    private const float FireWidth = 70f;
    private const float MagazineWidth = 70f;
    private const float ReloadWidth = 70f;
    private const float HitRateWidth = 100f;
    private const float HitCountWidth = 80f;
    private const float ExpectedDpsWidth = 100f;


    [MenuItem("Tools/Weapon Balance Viewer")]
    private static void Open()
    {
        GetWindow<WeaponBalanceWindow>("Weapon Balance");
    }


    private void OnEnable()
    {
        ReloadWeapons();
    }


    /// <summary>
    /// WeaponDataを取得
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
    /// WeaponDataのGUID取得
    /// </summary>
    private static string GetGuid(WeaponData weapon)
    {
        string path = AssetDatabase.GetAssetPath(weapon);
        return AssetDatabase.AssetPathToGUID(path);
    }


    /// <summary>
    /// 保存済み調整値を取得
    /// </summary>
    private BalanceInfo LoadBalanceInfo(WeaponData weapon)
    {
        string guid = GetGuid(weapon);

        return new BalanceInfo
        {
            HitRate = EditorPrefs.GetFloat($"{guid}_HitRate", 1f),
            HitCount = EditorPrefs.GetInt($"{guid}_HitCount", 1)
        };
    }


    /// <summary>
    /// 調整値を保存
    /// </summary>
    private void SaveBalanceInfo(WeaponData weapon, BalanceInfo info)
    {
        string guid = GetGuid(weapon);

        EditorPrefs.SetFloat($"{guid}_HitRate", info.HitRate);
        EditorPrefs.SetInt($"{guid}_HitCount", info.HitCount);
    }


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
            DrawWeaponRow(weapon);
        }

        EditorGUILayout.EndScrollView();
    }


    /// <summary>
    /// 表ヘッダー表示
    /// </summary>
    private void DrawHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        GUILayout.Label("Data", GUILayout.Width(IndexWidth));
        GUILayout.Label("Name", GUILayout.Width(NameWidth));
        GUILayout.Label("Type", GUILayout.Width(TypeWidth));
        GUILayout.Label("Damage", GUILayout.Width(DamageWidth));
        GUILayout.Label("Avg DPS", GUILayout.Width(AverageDpsWidth));
        GUILayout.Label("Fire", GUILayout.Width(FireWidth));
        GUILayout.Label("Magazine", GUILayout.Width(MagazineWidth));
        GUILayout.Label("Reload", GUILayout.Width(ReloadWidth));
        GUILayout.Label("Hit Rate", GUILayout.Width(HitRateWidth));
        GUILayout.Label("Hit Count", GUILayout.Width(HitCountWidth));
        GUILayout.Label("Expected DPS", GUILayout.Width(ExpectedDpsWidth));

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 武器1行分を表示
    /// </summary>
    private void DrawWeaponRow(WeaponData weapon)
    {
        BalanceInfo info = _balanceInfos[weapon];

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(weapon.name, GUILayout.Width(IndexWidth)))
        {
            Selection.activeObject = weapon;
            EditorGUIUtility.PingObject(weapon);
        }

        GUILayout.Label(weapon.DisplayName, GUILayout.Width(NameWidth));
        GUILayout.Label(weapon.WeaponType.ToString(), GUILayout.Width(TypeWidth));
        GUILayout.Label(weapon.Damage.ToString(), GUILayout.Width(DamageWidth));
        GUILayout.Label(weapon.AverageDPS.ToString("F1"), GUILayout.Width(AverageDpsWidth));
        GUILayout.Label(weapon.FireInterval.ToString("F2"), GUILayout.Width(FireWidth));
        GUILayout.Label(weapon.MagazineSize.ToString(), GUILayout.Width(MagazineWidth));
        GUILayout.Label(weapon.ReloadTime.ToString("F2"), GUILayout.Width(ReloadWidth));

        float hitRate = EditorGUILayout.Slider(
            info.HitRate,
            0f,
            1f,
            GUILayout.Width(HitRateWidth));

        if (!Mathf.Approximately(hitRate, info.HitRate))
        {
            info.HitRate = hitRate;
            SaveBalanceInfo(weapon, info);
        }

        int hitCount = EditorGUILayout.IntField(info.HitCount, GUILayout.Width(HitCountWidth));
        hitCount = Mathf.Max(1, hitCount);

        if (hitCount != info.HitCount)
        {
            info.HitCount = hitCount;
            SaveBalanceInfo(weapon, info);
        }

        GUILayout.Label(
            info.ExpectedDPS(weapon).ToString("F1"),
            GUILayout.Width(ExpectedDpsWidth));

        EditorGUILayout.EndHorizontal();
    }
}