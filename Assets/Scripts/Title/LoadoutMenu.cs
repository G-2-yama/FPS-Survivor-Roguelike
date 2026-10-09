using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutMenu : MonoBehaviour
{
    [Header("初期武器")]
    [Tooltip("選択肢として並べる武器 リストの順に配置されます")]
    [SerializeField] private List<WeaponData> startingWeapons = new List<WeaponData>();

    [Tooltip("武器の選択肢1つ分のプレハブ")]
    [SerializeField] private StartWeaponOptionView optionPrefab;

    [Tooltip("選択肢を並べる親 この ToggleGroup で1つだけ選べるようにします")]
    [SerializeField] private ToggleGroup optionGroup;

    [Header("シンクロ")]
    [Tooltip("シンクロの有無を切り替えるトグル")]
    [SerializeField] private Toggle weaponSyncToggle;

    private void Start()
    {
        CreateWeaponOptions();

        weaponSyncToggle.SetIsOnWithoutNotify(GameStartSettings.WeaponSync);
        weaponSyncToggle.onValueChanged.AddListener(isOn => GameStartSettings.WeaponSync = isOn);
    }

    private void CreateWeaponOptions()
    {
        var weapons = startingWeapons.Where(weaponData => weaponData != null).ToList();
        if (weapons.Count == 0)
            return;

        if (!weapons.Contains(GameStartSettings.StartingWeapon))
            GameStartSettings.StartingWeapon = weapons[0];

        foreach (var weaponData in weapons)
        {
            var option = Instantiate(optionPrefab, optionGroup.transform);
            option.Setup(
                weaponData,
                optionGroup,
                weaponData == GameStartSettings.StartingWeapon,
                () => GameStartSettings.StartingWeapon = weaponData);
        }
    }
}
