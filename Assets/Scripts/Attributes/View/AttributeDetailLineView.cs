using TMPro;
using UnityEngine;

public class AttributeDetailLineView : MonoBehaviour
{
    [Tooltip("行の頭の見出し（数・条件・区分・装備名）")]
    [SerializeField] private TMP_Text labelText;
    [Tooltip("行の本文")]
    [SerializeField] private TMP_Text bodyText;
    [Tooltip("行全体の透明度を変える")]
    [SerializeField] private CanvasGroup group;

    public void Setup(string label, string body, float alpha)
    {
        labelText.text = label;
        bodyText.text = body;
        group.alpha = alpha;
    }
}
