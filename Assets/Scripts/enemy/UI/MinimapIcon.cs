using UnityEngine;

public class MinimapIcon : PoolableObject
{
    public RectTransform RectTransform { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public override void OnGet()
    {
        gameObject.SetActive(true);
    }

    public override void OnRelease()
    {
        gameObject.SetActive(false);
    }
}