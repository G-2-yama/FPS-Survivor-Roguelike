using System;
using System.Collections.Generic;
using UnityEngine;

public class ModeSelectMenu : MonoBehaviour
{
    [Tooltip("選択肢として並べるモード リストの順に配置されます")]
    [SerializeField] private List<GameModeData> modes = new List<GameModeData>();

    [Tooltip("モードの選択肢1つ分のプレハブ")]
    [SerializeField] private GameModeOptionView optionPrefab;

    [Tooltip("選択肢を並べる親")]
    [SerializeField] private Transform optionParent;

    public event Action OnModeSelected;

    private void Start()
    {
        foreach (var mode in modes)
        {
            if (mode == null)
                continue;

            var option = Instantiate(optionPrefab, optionParent);
            option.Setup(mode, () => Select(mode));
        }
    }

    private void Select(GameModeData mode)
    {
        GameStartSettings.Mode = mode;
        OnModeSelected?.Invoke();
    }
}
