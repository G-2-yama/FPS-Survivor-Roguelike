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

    [Tooltip("選んでいる武器の名前・説明・アイコンを表示する欄")]
    [SerializeField] private StartWeaponDetailView detailView;

    [Header("シンクロ")]
    [Tooltip("シンクロをオンにするボタン")]
    [SerializeField] private TitleChoiceView weaponSyncOnButton;

    [Tooltip("シンクロをオフにするボタン")]
    [SerializeField] private TitleChoiceView weaponSyncOffButton;

    private void Start()
    {
        CreateWeaponOptions();

        weaponSyncOnButton.Button.onClick.AddListener(() => SetWeaponSync(true));
        weaponSyncOffButton.Button.onClick.AddListener(() => SetWeaponSync(false));
        SetWeaponSync(GameStartSettings.WeaponSync);
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
                () => SelectWeapon(weaponData));
        }

        detailView.Show(GameStartSettings.StartingWeapon);
    }

    private void SelectWeapon(WeaponData weaponData)
    {
        GameStartSettings.StartingWeapon = weaponData;
        detailView.Show(weaponData);
    }

    private void SetWeaponSync(bool isOn)
    {
        GameStartSettings.WeaponSync = isOn;
        weaponSyncOnButton.SetOn(isOn);
        weaponSyncOffButton.SetOn(!isOn);
    }
}
