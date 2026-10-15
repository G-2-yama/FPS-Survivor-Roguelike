using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>プレイ中の装備とプレイヤーの成長状態を変更するデバッグ画面</summary>
public class DebugLoadoutWindow : EditorWindow
{
    private Dictionary<WeaponSlotGroup, List<WeaponData>> weaponDefinitions;
    private List<Item> itemDefinitions;

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
        weaponDefinitions = LoadAll<WeaponData>()
            .Where(w => !w.IsEmpty)
            .GroupBy(w => ToSlotGroup(w.WeaponType))
            .ToDictionary(g => g.Key, g => g.ToList());
        itemDefinitions = LoadAll<Item>().ToList();
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
        if (!weaponDefinitions.TryGetValue(slot.GetGroup(), out List<WeaponData> definitions))
        {
            definitions = new List<WeaponData>();
        }

        Weapon weapon = inventory.GetWeapon(slot);
        WeaponData current = weapon != null && weapon.HasWeapon ? weapon.WeaponData : null;
        int currentLevel = current != null ? weapon.Level : 0;

        if (DrawLevelSelector(slot.ToString(), definitions, current, currentLevel, w => w.DisplayName, WeaponLevelLabels,
                out WeaponData selected, out int selectedLevel))
        {
            pendingChange = () => inventory.EquipWeapon(slot, selected, selectedLevel);
        }
    }

    /// <summary>武器のレベル選択欄に使う表示を組み立てる</summary>
    private static string[] WeaponLevelLabels(WeaponData data)
    {
        return Enumerable.Range(0, Mathf.Max(1, data.LevelCount))
            .Select(i => data.IsEvoLevel(i) ? "Evo" : $"Lv{i + 1}")
            .ToArray();
    }

    /// <summary>アイテムのレベル選択欄に使う表示を組み立てる</summary>
    private static string[] ItemLevelLabels(Item item)
    {
        return Enumerable.Range(0, Mathf.Max(1, item.MaxLevel))
            .Select(i => $"Lv{i + 1}")
            .ToArray();
    }

    /// <summary>指定スロットのアイテム選択を表示し、装備変更を予約する</summary>
    private void DrawItemSlot(PlayerInventory inventory, int index)
    {
        OwnedItem owned = inventory.Items[index];
        Item current = owned?.Item;
        int currentLevel = owned?.Level ?? 0;
        if (!DrawLevelSelector($"Item {index}", itemDefinitions, current, currentLevel, i => i.DisplayName, ItemLevelLabels,
                out Item selected, out int selectedLevel))
        {
            return;
        }

        (Item item, int level)[] desired = inventory.Items
            .Select(o => o != null ? (o.Item, o.Level) : ((Item)null, 0))
            .ToArray();
        desired[index] = (selected, selectedLevel);
        pendingChange = () => ReplaceItems(inventory, desired);
    }

    /// <summary>現在のアイテムを解除してから選択された構成を装備する</summary>
    private static void ReplaceItems(PlayerInventory inventory, (Item item, int level)[] desired)
    {
        for (int i = inventory.Items.Count - 1; i >= 0; i--)
        {
            inventory.DiscardItem(i);
        }

        foreach ((Item item, int level) in desired.Where(d => d.item != null))
        {
            inventory.EquipItem(item, level);
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

    /// <summary>種類とレベルの選択欄を表示し、変更があれば選択された定義とレベルを返す</summary>
    private static bool DrawLevelSelector<T>(string label, List<T> definitions, T current, int currentLevel,
        Func<T, string> displayName, Func<T, string[]> levelLabels, out T selected, out int selectedLevel) where T : Object
    {
        selected = current;
        selectedLevel = currentLevel;

        int definitionIndex = current != null ? definitions.IndexOf(current) : -1;

        string[] definitionLabels = new[] { "なし" }
            .Concat(definitions.Select(definition => DefinitionLabel(definition, displayName)))
            .ToArray();

        using (new EditorGUILayout.HorizontalScope())
        {
            int newDefinitionIndex = EditorGUILayout.Popup(label, definitionIndex + 1, definitionLabels) - 1;
            if (newDefinitionIndex != definitionIndex)
            {
                selected = newDefinitionIndex >= 0 ? definitions[newDefinitionIndex] : null;
                selectedLevel = 0;
                return true;
            }

            if (definitionIndex < 0)
            {
                return false;
            }

            string[] labels = levelLabels(current);
            int newLevel = EditorGUILayout.Popup(Mathf.Clamp(currentLevel, 0, labels.Length - 1), labels, GUILayout.Width(160));
            if (newLevel == currentLevel)
            {
                return false;
            }

            selectedLevel = newLevel;
            return true;
        }
    }

    /// <summary>種類の選択欄に使う表示名とアセット名を組み立てる</summary>
    private static string DefinitionLabel<T>(T root, Func<T, string> displayName) where T : Object
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
}
