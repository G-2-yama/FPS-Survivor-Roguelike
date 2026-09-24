using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>現在の操作方式に対応するキー・ボタン名を表示する。</summary>
public class KeyBindingView : MonoBehaviour
{
    [SerializeField] private Text keyBindingText;
    [SerializeField] private InputActionReference inputAction;
    [SerializeField] private PlayerInput playerInput;

    private void OnEnable()
    {
        InputSystem.onActionChange += OnInputActionChange;
        RefreshKeyBinding();
    }

    private void Start()
    {
        RefreshKeyBinding();
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= OnInputActionChange;
    }

    public void RefreshKeyBinding()
    {
        var action = playerInput.actions.FindAction(inputAction.action.id.ToString(), throwIfNotFound: true);
        string group = playerInput.currentControlScheme == "Gamepad" ? "Gamepad" : "Keyboard&Mouse";
        keyBindingText.text = action.GetBindingDisplayString(group: group);
    }

    private void OnInputActionChange(object changedObject, InputActionChange change)
    {
        if (change == InputActionChange.BoundControlsChanged)
            RefreshKeyBinding();
    }
}
