using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>プレイ中の装備とプレイヤーの成長状態を変更するデバッグ画面</summary>
public class DebugLoadoutWindow : EditorWindow
{
    private Dictionary<WeaponSlotGroup, List<List<WeaponData>>> weaponChains;
    private List<List<Item>> itemChains;

    private Player player;
    private GameManager gameManager;
    private Vector2 scroll;
    private Action pendingChange;

    /// <summary>メニューからデバッグ画面を開く</summary>
    [MenuItem("Tools/Debug Loadout")]
    private static void Open()
    {
        GetWindow<DebugLoadoutWindow>("Debug Loadout");
    }

    /// <summary>武器とアイテムの強化段階を読み込み、選択肢を準備する</summary>
    private void OnEnable()
    {
        List<List<WeaponData>> allWeaponChains = BuildChains(LoadAll<WeaponData>().Where(w => !w.IsEmpty), w => w.NextLevelData);
        weaponChains = allWeaponChains
            .GroupBy(chain => ToSlotGroup(chain[0].WeaponType))
            .ToDictionary(g => g.Key, g => g.ToList());
        itemChains = BuildChains(LoadAll<Item>(), i => i.NextLevelItem);
    }

    /// <summary>プレイ中の状態変化を画面に反映する</summary>
    private void OnInspectorUpdate()
    {
        if (Application.isPlaying)
        {
            Repaint();
        }
    }

    /// <summary>装備と成長状態を表示し、描画完了後に変更を適用する</summary>
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

        // 装備変更やレベルアップ通知で画面構成が変わるため、レイアウトを閉じてから実行する
        if (pendingChange != null)
        {
            Action change = pendingChange;
            pendingChange = null;
            change();
            Repaint();
        }
    }

    /// <summary>指定スロットの武器選択を表示し、装備変更を予約する</summary>
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

    /// <summary>指定スロットのアイテム選択を表示し、装備変更を予約する</summary>
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

    /// <summary>現在のアイテムを解除してから選択された構成を装備する</summary>
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

    /// <summary>プレイヤーの成長状態を表示し、必要経験値の加算を予約する</summary>
    private void DrawPlayerLevel()
    {
        if (player.Stats == null)
        {
            EditorGUILayout.HelpBox("Player の初期化が完了していません", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("レベル", player.Level.ToString());
        EditorGUILayout.LabelField("経験値", $"{player.Exp:0.#} / {player.LevelUpRequiredExp:0.#}");
        EditorGUILayout.LabelField("未消化のレベルアップ", player.PendingLevelUps.ToString());

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<GameManager>();
        }

        GameStateMachine stateMachine = gameManager != null ? gameManager.GameStateMachine : null;
        bool isPlaying = stateMachine != null && stateMachine.CurrentState == stateMachine.PlayingState;

        using (new EditorGUI.DisabledScope(!isPlaying))
        {
            if (GUILayout.Button("レベルアップ"))
            {
                // 成長状態を管理するPlayerへ経験値を渡し、通常の強化選択とUI通知も発生させる
                pendingChange = () => player.AddExp(player.LevelUpRequiredExp);
            }
        }
    }

    /// <summary>種類と強化段階の選択欄を表示し、選択されたアセットを返す</summary>
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

    /// <summary>種類の選択欄に使う表示名とアセット名を組み立てる</summary>
    private static string ChainLabel<T>(T root, Func<T, string> displayName) where T : Object
    {
        string name = displayName(root);
        return string.IsNullOrEmpty(name) ? root.name : $"{name} ({root.name})";
    }

    /// <summary>武器の種類を対応するスロット群へ変換する</summary>
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

    /// <summary>指定型のアセットをパス順で読み込み、重複と欠損を除外する</summary>
    private static IEnumerable<T> LoadAll<T>() where T : Object
    {
        return AssetDatabase.FindAssets($"t:{typeof(T).Name}")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct()
            .OrderBy(path => path)
            .Select(AssetDatabase.LoadAssetAtPath<T>)
            .Where(asset => asset != null);
    }

    /// <summary>次段階への参照をたどり、種類ごとの強化段階を構築する</summary>
    private static List<List<T>> BuildChains<T>(IEnumerable<T> assets, Func<T, T> next) where T : Object
    {
        List<T> all = assets.ToList();
        HashSet<T> hasPrevious = new HashSet<T>(all.Select(next).Where(asset => asset != null));

        List<List<T>> chains = new List<List<T>>();
        foreach (T root in all.Where(asset => !hasPrevious.Contains(asset)))
        {
            List<T> chain = new List<T>();
            // 誤って循環する参照が設定されていても同じアセットで走査を止める
            for (T asset = root; asset != null && !chain.Contains(asset); asset = next(asset))
            {
                chain.Add(asset);
            }
            chains.Add(chain);
        }
        return chains;
    }
}
