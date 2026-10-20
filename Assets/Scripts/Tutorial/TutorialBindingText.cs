using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class TutorialBindingText : MonoBehaviour
{
    private const string GamepadGroup = "Gamepad";
    private const string KeyboardMouseGroup = "Keyboard&Mouse";

    private const string HoldPrefix = "長押し ";

    private static readonly Regex Placeholder = new Regex(@"\{(\w+)\}");

    private static readonly List<string> DirectionOrder = new List<string> { "up", "left", "down", "right" };

    private static readonly Dictionary<string, string> DisplayNameOverrides = new Dictionary<string, string>
    {
        { "<Pointer>/delta", "マウス" },
        { "<Gamepad>/leftStickPress", "LS押し込み" },
    };

    [Tooltip("キー名・ボタン名を差し込むテキスト")]
    [SerializeField] private TMP_Text text;

    [Tooltip("{アクション名} の部分を、最後に触った機器のキー名・ボタン名に置き換える。例: {Move} で移動、{Jump} でジャンプ")]
    [TextArea(2, 6)]
    [SerializeField] private string template;

    private PlayerInput playerInput;

    public string Template
    {
        get => template;
        set
        {
            template = value;
            Refresh();
        }
    }

    private void OnEnable()
    {
        InputSystem.onActionChange += OnInputActionChange;
        if (playerInput != null)
            playerInput.controlsChangedEvent.AddListener(OnControlsChanged);
        Refresh();
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnInputActionChange;
        if (playerInput != null)
            playerInput.controlsChangedEvent.RemoveListener(OnControlsChanged);
    }

    private void Update()
    {
        if (playerInput != null)
            return;

        playerInput = FindFirstObjectByType<PlayerInput>();
        if (playerInput == null)
            return;

        playerInput.controlsChangedEvent.AddListener(OnControlsChanged);
        Refresh();
    }

    public void Refresh()
    {
        if (text == null || playerInput == null)
            return;

        string group = playerInput.currentControlScheme == GamepadGroup ? GamepadGroup : KeyboardMouseGroup;
        text.text = Placeholder.Replace(template ?? string.Empty, match => ToDisplayString(match, group));
    }

    private string ToDisplayString(Match match, string group)
    {
        string actionName = match.Groups[1].Value;
        InputAction action = playerInput.actions.FindAction(actionName);
        if (action == null)
        {
            Debug.LogWarning($"アクション {actionName} が入力設定にありません", this);
            return match.Value;
        }

        InputBinding mask = InputBinding.MaskByGroup(group);
        ReadOnlyArray<InputBinding> bindings = action.bindings;
        for (int i = 0; i < bindings.Count; i++)
        {
            InputBinding binding = bindings[i];
            if (binding.isPartOfComposite)
                continue;

            string display = null;
            if (binding.isComposite)
                display = CompositeDisplayString(action, i, mask);
            else if (mask.Matches(binding))
                display = ControlDisplayString(action, i);

            if (display == null)
                continue;

            return IsHold(action, binding) ? $"{HoldPrefix}{display}" : display;
        }

        return string.Empty;
    }

    private static string CompositeDisplayString(InputAction action, int compositeIndex, InputBinding mask)
    {
        ReadOnlyArray<InputBinding> bindings = action.bindings;
        var partNames = new List<string>();
        var partDisplays = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (int i = compositeIndex + 1; i < bindings.Count && bindings[i].isPartOfComposite; i++)
        {
            InputBinding part = bindings[i];
            if (!mask.Matches(part) || partDisplays.ContainsKey(part.name))
                continue;

            partNames.Add(part.name);
            partDisplays.Add(part.name, ControlDisplayString(action, i));
        }

        if (partNames.Count == 0)
            return null;

        return string.Join("/", partNames.OrderBy(DirectionRank).Select(name => partDisplays[name]));
    }

    private static int DirectionRank(string partName)
    {
        int index = DirectionOrder.IndexOf(partName.ToLowerInvariant());
        return index >= 0 ? index : DirectionOrder.Count;
    }

    private static string ControlDisplayString(InputAction action, int bindingIndex)
    {
        if (DisplayNameOverrides.TryGetValue(action.bindings[bindingIndex].effectivePath, out string name))
            return name;

        return action.GetBindingDisplayString(bindingIndex, InputBinding.DisplayStringOptions.DontIncludeInteractions);
    }

    private static bool IsHold(InputAction action, InputBinding binding)
    {
        return ContainsHold(binding.effectiveInteractions) || ContainsHold(action.interactions);
    }

    private static bool ContainsHold(string interactions)
    {
        return !string.IsNullOrEmpty(interactions)
            && interactions.IndexOf("Hold", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void OnControlsChanged(PlayerInput input)
    {
        Refresh();
    }

    private void OnInputActionChange(object changedObject, InputActionChange change)
    {
        if (change == InputActionChange.BoundControlsChanged)
            Refresh();
    }
}
