using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialWeaponRow : SettingsMenuItem
{
    [Header("装備")]
    [Tooltip("この行で付け替えるスロット 候補はスロットと同じ種類の武器と「なし」です")]
    [SerializeField] private SlotType slot;

    [Header("表示")]
    [Tooltip("今の武器の名前")]
    [SerializeField] private TMP_Text nameText;

    [Tooltip("前の武器に替えるボタン")]
    [SerializeField] private Button decreaseButton;

    [Tooltip("次の武器に替えるボタン")]
    [SerializeField] private Button increaseButton;

    [Tooltip("前に替えるボタンの三角")]
    [SerializeField] private Graphic decreaseArrow;

    [Tooltip("次に替えるボタンの三角")]
    [SerializeField] private Graphic increaseArrow;

    [Header("色と大きさ")]
    [Tooltip("武器の名前の色")]
    [SerializeField] private Color nameColor = new Color(0.36f, 0.78f, 1f);

    [Tooltip("武器が無いときの「なし」の色")]
    [SerializeField] private Color emptyColor = new Color(0.54f, 0.56f, 0.62f);

    [Tooltip("行を選んでいないときの三角の色")]
    [SerializeField] private Color arrowColor = new Color(0.54f, 0.56f, 0.62f);

    [Tooltip("行を選んでいるときの三角の色")]
    [SerializeField] private Color arrowFocusedColor = new Color(0.96f, 0.96f, 0.97f);

    [Tooltip("武器を替えた瞬間の三角の色")]
    [SerializeField] private Color arrowHitColor = new Color(0.36f, 0.78f, 1f);

    [Tooltip("武器を替えた瞬間の三角の拡大率")]
    [SerializeField] private float arrowHitScale = 1.3f;

    [Tooltip("武器を替えた瞬間の見た目を保つ時間（秒）")]
    [SerializeField] private float arrowHitDuration = 0.12f;

    private const string EmptyName = "なし";

    private readonly List<WeaponData> candidates = new List<WeaponData>();
    private PlayerInventory inventory;
    private bool isFocused;
    private float decreaseHitAt = float.NegativeInfinity;
    private float increaseHitAt = float.NegativeInfinity;

    public void Bind(PlayerInventory playerInventory, IReadOnlyList<WeaponData> weapons)
    {
        Unsubscribe();
        inventory = playerInventory;

        candidates.Clear();
        candidates.Add(null);
        WeaponType type = ToWeaponType(slot.GetGroup());
        foreach (var weapon in weapons)
        {
            if (weapon != null && !weapon.IsEmpty && weapon.WeaponType == type && !candidates.Contains(weapon))
                candidates.Add(weapon);
        }

        if (isActiveAndEnabled)
        {
            Subscribe();
            Refresh();
        }
    }

    private void Awake()
    {
        decreaseButton.onClick.AddListener(() => Step(-1));
        increaseButton.onClick.AddListener(() => Step(1));
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
        decreaseHitAt = increaseHitAt = float.NegativeInfinity;
        UpdateArrows();
    }

    private void Update()
    {
        UpdateArrows();
    }

    public override void SetFocused(bool focused)
    {
        base.SetFocused(focused);
        isFocused = focused;
    }

    public override void Step(int direction)
    {
        if (inventory == null || candidates.Count == 0 || direction == 0)
            return;

        if (direction < 0)
            decreaseHitAt = Time.unscaledTime;
        else
            increaseHitAt = Time.unscaledTime;

        int index = candidates.IndexOf(CurrentWeapon());
        if (index < 0)
            index = 0;
        int next = (index + (direction > 0 ? 1 : -1) + candidates.Count) % candidates.Count;
        inventory.EquipWeapon(slot, candidates[next]);
    }

    private WeaponData CurrentWeapon()
    {
        WeaponData data = inventory != null ? inventory.GetWeaponData(slot) : null;
        return data != null && !data.IsEmpty ? data : null;
    }

    private void Subscribe()
    {
        if (inventory != null)
            inventory.OnSlotChanged += OnSlotChanged;
    }

    private void Unsubscribe()
    {
        if (inventory != null)
            inventory.OnSlotChanged -= OnSlotChanged;
    }

    private void OnSlotChanged(SlotType changedSlot, WeaponData data)
    {
        if (changedSlot == slot)
            Refresh();
    }

    private void Refresh()
    {
        WeaponData current = CurrentWeapon();
        nameText.text = current != null ? current.DisplayName : EmptyName;
        nameText.color = current != null ? nameColor : emptyColor;
    }

    private void UpdateArrows()
    {
        ApplyArrow(decreaseArrow, decreaseHitAt);
        ApplyArrow(increaseArrow, increaseHitAt);
    }

    private void ApplyArrow(Graphic arrow, float hitAt)
    {
        bool isHit = Time.unscaledTime - hitAt < arrowHitDuration;
        arrow.color = isHit ? arrowHitColor : isFocused ? arrowFocusedColor : arrowColor;
        arrow.rectTransform.localScale = Vector3.one * (isHit ? arrowHitScale : 1f);
    }

    private static WeaponType ToWeaponType(WeaponSlotGroup group)
    {
        switch (group)
        {
            case WeaponSlotGroup.Ability:
                return WeaponType.Ability;
            case WeaponSlotGroup.AutoWeapon:
                return WeaponType.AutoWeapon;
            default:
                return WeaponType.Main;
        }
    }
}
