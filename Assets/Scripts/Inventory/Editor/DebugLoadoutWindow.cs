using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class DebugLoadoutWindow : EditorWindow
{
    private Dictionary<WeaponSlotGroup, List<List<WeaponData>>> weaponChains;
    private List<List<Item>> itemChains;

    private Player player;
    private ExpManager expManager;
    private Vector2 scroll;
    private Action pendingChange;

    [MenuItem("Tools/Debug Loadout")]
    private static void Open()
    {
        GetWindow<DebugLoadoutWindow>("Debug Loadout");
    }

    private void OnEnable()
    {
        List<List<WeaponData>> allWeaponChains = BuildChains(LoadAll<WeaponData>().Where(w => !w.IsEmpty), w => w.NextLevelData);
        weaponChains = allWeaponChains
            .GroupBy(chain => ToSlotGroup(chain[0].WeaponType))
            .ToDictionary(g => g.Key, g => g.ToList());
        itemChains = BuildChains(LoadAll<Item>(), i => i.NextLevelItem);
    }

    private void OnInspectorUpdate()
    {
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    private void OnGUI()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("プレイ中のみ使えます", MessageType.Info);
            return;
        }

        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        if (player == null || player.Inventory == null)
        {
            EditorGUILayout.HelpBox("Player が見つかりません", MessageType.Warning);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("武器", EditorStyles.boldLabel);
        foreach (SlotType slot in Enum.GetValues(typeof(SlotType)))
        {
            DrawWeaponSlot(player.Inventory, slot);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("アイテム", EditorStyles.boldLabel);
        for (int i = 0; i < player.Inventory.Items.Count; i++)
        {
            DrawItemSlot(player.Inventory, i);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("プレイヤー", EditorStyles.boldLabel);
        DrawPlayerLevel();

        EditorGUILayout.EndScrollView();

        if (pendingChange != null)
        {
            Action change = pendingChange;
            pendingChange = null;
            change();
            Repaint();
        }
    }

    private void DrawWeaponSlot(PlayerInventory inventory, SlotType slot)
    {
        if (!weaponChains.TryGetValue(slot.GetGroup(), out List<List<WeaponData>> chains))
        {
            chains = new List<List<WeaponData>>();
        }

        WeaponData current = inventory.GetWeaponData(slot);
        if (current != null && current.IsEmpty)
        {
            current = null;
        }

        WeaponData selected = DrawChainSelector(slot.ToString(), chains, current, w => w.DisplayName);
        if (selected != current)
        {
            pendingChange = () => inventory.EquipWeapon(slot, selected);
        }
    }

    private void DrawItemSlot(PlayerInventory inventory, int index)
    {
        Item current = inventory.Items[index];
        Item selected = DrawChainSelector($"Item {index}", itemChains, current, i => i.DisplayName);
        if (selected == current)
        {
            return;
        }

        Item[] desired = inventory.Items.ToArray();
        desired[index] = selected;
        pendingChange = () => ReplaceItems(inventory, desired);
    }

    private static void ReplaceItems(PlayerInventory inventory, Item[] desired)
    {
        for (int i = inventory.Items.Count - 1; i >= 0; i--)
        {
            inventory.DiscardItem(i);
        }

        foreach (Item item in desired.Where(item => item != null))
        {
            inventory.EquipItem(item);
        }
    }

    private void DrawPlayerLevel()
    {
        if (expManager == null)
        {
            expManager = FindAnyObjectByType<ExpManager>();
        }

        string required = expManager != null ? expManager.LevelUpRequiredExp.ToString("0.#") : "?";
        EditorGUILayout.LabelField("レベル", player.Level.ToString());
        EditorGUILayout.LabelField("経験値", $"{player.Exp:0.#} / {required}");
        EditorGUILayout.LabelField("未消化のレベルアップ", player.PendingLevelUps.ToString());

        using (new EditorGUI.DisabledScope(expManager == null))
        {
            if (GUILayout.Button("レベルアップ"))
            {
                pendingChange = () => player.AddExp(expManager.LevelUpRequiredExp);
            }
        }
    }

    private static T DrawChainSelector<T>(string label, List<List<T>> chains, T current, Func<T, string> displayName) where T : Object
    {
        int chainIndex = chains.FindIndex(chain => chain.Contains(current));
        int levelIndex = chainIndex >= 0 ? chains[chainIndex].IndexOf(current) : -1;

        string[] chainLabels = new[] { "なし" }
            .Concat(chains.Select(chain => ChainLabel(chain[0], displayName)))
            .ToArray();

        using (new EditorGUILayout.HorizontalScope())
        {
            int newChainIndex = EditorGUILayout.Popup(label, chainIndex + 1, chainLabels) - 1;
            if (newChainIndex != chainIndex)
            {
                return newChainIndex >= 0 ? chains[newChainIndex][0] : null;
            }

            if (chainIndex < 0)
            {
                return current;
            }

            List<T> chain = chains[chainIndex];
            string[] levelLabels = chain.Select((asset, i) => $"Lv{i + 1}  {asset.name}").ToArray();
            int newLevelIndex = EditorGUILayout.Popup(levelIndex, levelLabels, GUILayout.Width(160));
            return chain[newLevelIndex];
        }
    }

    private static string ChainLabel<T>(T root, Func<T, string> displayName) where T : Object
    {
        string name = displayName(root);
        return string.IsNullOrEmpty(name) ? root.name : $"{name} ({root.name})";
    }

    private static WeaponSlotGroup ToSlotGroup(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Main:
                return WeaponSlotGroup.Main;
            case WeaponType.Ability:
                return WeaponSlotGroup.Ability;
            case WeaponType.AutoWeapon:
                return WeaponSlotGroup.AutoWeapon;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private static IEnumerable<T> LoadAll<T>() where T : Object
    {
        return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct()
            .OrderBy(path => path)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(asset => asset != null);
    }

    private static List<List<T>> BuildChains<T>(IEnumerable<T> assets, Func<T, T> next) where T : Object
    {
        List<T> all = assets.ToList();
        HashSet<T> hasPrevious = new HashSet<T>(all.Select(next).Where(asset => asset != null));

        List<List<T>> chains = new List<List<T>>();
        foreach (T root in all.Where(asset => !hasPrevious.Contains(asset)))
        {
            List<T> chain = new List<T>();
            for (T asset = root; asset != null && !chain.Contains(asset); asset = next(asset))
            {
                chain.Add(asset);
            }
            chains.Add(chain);
        }
        return chains;
    }
}
