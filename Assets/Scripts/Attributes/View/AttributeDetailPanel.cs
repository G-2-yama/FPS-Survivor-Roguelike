using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AttributeDetailPanel : MonoBehaviour
{
    [Header("Parts")]
    [Tooltip("パネルの本体。高さを中身に合わせて変える")]
    [SerializeField] private RectTransform panel;
    [Tooltip("上段。バッジ・ゲージ・数を並べる")]
    [SerializeField] private RectTransform head;
    [Tooltip("上段の属性の行")]
    [SerializeField] private AttributeRowView headRow;
    [Tooltip("上段の数（5/7）")]
    [SerializeField] private TMP_Text countText;
    [Tooltip("閉じるボタン")]
    [SerializeField] private Button closeButton;
    [Tooltip("本文のスクロール")]
    [SerializeField] private ScrollRect scroll;
    [Tooltip("スクロールする中身")]
    [SerializeField] private RectTransform content;

    [Header("Sections")]
    [Tooltip("固有効果の見出しと行のまとまり。出す行が無ければ隠す")]
    [SerializeField] private GameObject traitSection;
    [Tooltip("固有効果の行を並べる親")]
    [SerializeField] private RectTransform traitList;
    [Tooltip("効果の行を並べる親")]
    [SerializeField] private RectTransform effectList;
    [Tooltip("所属する装備の行を並べる親")]
    [SerializeField] private RectTransform memberList;
    [Tooltip("固有効果と効果の行の見本")]
    [SerializeField] private AttributeDetailLineView effectLineTemplate;
    [Tooltip("所属する装備の行の見本")]
    [SerializeField] private AttributeDetailLineView memberLineTemplate;

    [Header("Size")]
    [Tooltip("上段の高さ。パネルの高さは、これと本文の高さの和")]
    [SerializeField, Min(0f)] private float headHeight = 132f;
    [Tooltip("パネルの高さの最小")]
    [SerializeField, Min(0f)] private float minHeight = 900f;
    [Tooltip("パネルの高さの最大。超えた分は本文をスクロールする")]
    [SerializeField, Min(0f)] private float maxHeight = 1000f;

    [Header("Style")]
    [Tooltip("属性と Evo の色")]
    [SerializeField] private UIPalette palette;
    [Tooltip("届いていない行と、持っていない装備の透明度")]
    [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.36f;
    [Tooltip("条件の文の大きさ（本文に対する %）")]
    [SerializeField, Min(1f)] private float conditionSizePercent = 85f;
    [Tooltip("条件のある効果の行の頭に出す文字")]
    [SerializeField] private string conditionLabel = "条件";
    [Tooltip("条件の文の前に付ける文字")]
    [SerializeField] private string conditionPrefix = "条件：";
    [Tooltip("固有効果の装備名と説明の区切り")]
    [SerializeField] private string traitSeparator = "：";
    [Tooltip("Evo したメインの名前の後ろに付ける文字")]
    [SerializeField] private string evoSuffix = " (+2)";
    [Tooltip("取ると加わる装備の名前の後ろに付ける文字")]
    [SerializeField] private string addedSuffix = "（取ると追加）";
    [Tooltip("所属する装備の名前の区切り")]
    [SerializeField] private string memberSeparator = "　 ";

    [Header("Categories")]
    [Tooltip("所属する装備の、メイン武器の行の見出し")]
    [SerializeField] private string mainLabel = "メイン";
    [Tooltip("所属する装備の、アビリティの行の見出し")]
    [SerializeField] private string abilityLabel = "アビリティ";
    [Tooltip("所属する装備の、オート武器の行の見出し")]
    [SerializeField] private string autoLabel = "オート";
    [Tooltip("所属する装備の、アイテムの行の見出し")]
    [SerializeField] private string itemLabel = "アイテム";

    private readonly Dictionary<RectTransform, List<AttributeDetailLineView>> lines = new Dictionary<RectTransform, List<AttributeDetailLineView>>();

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
    }

    public void Show(AttributeDefinition attribute, AttributeSnapshot snapshot)
    {
        if (attribute == null || snapshot == null)
            return;

        gameObject.SetActive(true);

        int count = snapshot.GetCount(attribute);
        headRow.ShowCount(attribute, count);
        int total = attribute.Kind == AttributeKind.Base ? attribute.MaxThreshold : attribute.ActivateThreshold;
        countText.text = $"{count}/{total}";
        countText.color = AttributeDisplayStates.Of(attribute, count).ToColor(palette, palette.AttributeIdleColor);

        var traits = TraitLines(attribute, snapshot).ToList();
        traitSection.SetActive(traits.Count > 0);
        Fill(traitList, effectLineTemplate, traits);
        Fill(effectList, effectLineTemplate, EffectLines(attribute, count).ToList());
        Fill(memberList, memberLineTemplate, MemberLines(attribute, snapshot).ToList());

        Resize();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private IEnumerable<(string label, string body, float alpha)> TraitLines(AttributeDefinition attribute, AttributeSnapshot snapshot)
    {
        if (attribute.Kind != AttributeKind.Base || snapshot.Catalog == null)
            yield break;

        foreach (WeaponData weapon in snapshot.Catalog.Weapons)
        {
            if (weapon == null || weapon.WeaponType != WeaponType.Main || !weapon.Attributes.Contains(attribute))
                continue;

            float alpha = snapshot.TryGetLevel(weapon, out _) ? 1f : dimAlpha;
            foreach (WeaponTrait trait in weapon.Traits)
            {
                if (trait != null && !string.IsNullOrEmpty(trait.Description))
                    yield return ("", weapon.DisplayName + traitSeparator + trait.Description, alpha);
            }
        }
    }

    private IEnumerable<(string label, string body, float alpha)> EffectLines(AttributeDefinition attribute, int count)
    {
        foreach (AttributeEffectText effect in attribute.EffectTexts)
        {
            if (effect == null)
                continue;

            string label = effect.HasCondition ? conditionLabel : effect.Count.ToString();
            string body = effect.HasCondition
                ? $"{effect.Text}\n<size={conditionSizePercent}%>{conditionPrefix}{effect.Condition}</size>"
                : effect.Text;
            yield return (label, body, count >= effect.Count ? 1f : dimAlpha);
        }
    }

    private IEnumerable<(string label, string body, float alpha)> MemberLines(AttributeDefinition attribute, AttributeSnapshot snapshot)
    {
        if (snapshot.Catalog == null)
            yield break;

        foreach ((WeaponType type, string label) in new[] { (WeaponType.Main, mainLabel), (WeaponType.Ability, abilityLabel), (WeaponType.AutoWeapon, autoLabel) })
        {
            var names = snapshot.Catalog.Weapons
                .Where(w => w != null && w.WeaponType == type && w.Attributes.Contains(attribute))
                .Select(w => WeaponName(w, snapshot))
                .ToList();
            if (names.Count > 0)
                yield return (label, string.Join(memberSeparator, names), 1f);
        }

        bool isBase = attribute.Kind == AttributeKind.Base;
        var itemNames = snapshot.Catalog.Items
            .Where(i => i != null && (i.Attributes.Contains(attribute) || (isBase && i.CountsAsAllBases)))
            .Select(i => MemberName(i.DisplayName, snapshot.Has(i), snapshot.IsAdded(i)))
            .ToList();
        if (itemNames.Count > 0)
            yield return (itemLabel, string.Join(memberSeparator, itemNames), 1f);
    }

    private string WeaponName(WeaponData weapon, AttributeSnapshot snapshot)
    {
        bool owned = snapshot.TryGetLevel(weapon, out int level);
        string name = weapon.DisplayName;
        if (owned && weapon.WeaponType == WeaponType.Main && weapon.IsEvoLevel(level))
            name = palette.ColorizeEvo(name + evoSuffix);
        return MemberName(name, owned, snapshot.IsAdded(weapon));
    }

    private string MemberName(string name, bool owned, bool added)
    {
        if (!owned)
            return $"<nobr><alpha=#{AlphaHex(dimAlpha)}>{name}<alpha=#FF></nobr>";
        if (added)
            return $"<nobr><color=#{ColorUtility.ToHtmlStringRGBA(palette.AttributeActiveColor)}>{name}{addedSuffix}</color></nobr>";
        return $"<nobr>{name}</nobr>";
    }

    private static string AlphaHex(float alpha) => Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f).ToString("X2");

    private void Fill(RectTransform parent, AttributeDetailLineView template, List<(string label, string body, float alpha)> rows)
    {
        if (!lines.TryGetValue(parent, out List<AttributeDetailLineView> pool))
        {
            pool = new List<AttributeDetailLineView>();
            lines[parent] = pool;
        }

        while (pool.Count < rows.Count)
            pool.Add(Instantiate(template, parent));

        for (int i = 0; i < pool.Count; i++)
        {
            bool used = i < rows.Count;
            pool[i].gameObject.SetActive(used);
            if (used)
                pool[i].Setup(rows[i].label, rows[i].body, rows[i].alpha);
        }
    }

    private void Resize()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(head);
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        float height = headHeight + LayoutUtility.GetPreferredHeight(content);
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Clamp(height, minHeight, Mathf.Max(minHeight, maxHeight)));
        scroll.verticalNormalizedPosition = 1f;
    }
}
