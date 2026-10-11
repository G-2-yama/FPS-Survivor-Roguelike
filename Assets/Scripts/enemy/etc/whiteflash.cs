using UnityEngine;

public class WhiteFlash : MonoBehaviour, ILifetimeOwner
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material whiteFlashMaterial;
    [SerializeField] private float flashDuration = 0.08f;

    private Material[][] originalMaterials;
    private Material[][] whiteMaterials;
    private bool isFlashing;

    public int LifetimeIndex { get; set; } = -1;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        originalMaterials = new Material[renderers.Length][];
        whiteMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterials;

            whiteMaterials[i] = new Material[originalMaterials[i].Length];

            for (int j = 0; j < whiteMaterials[i].Length; j++)
            {
                whiteMaterials[i][j] = whiteFlashMaterial;
            }
        }
    }

    public void Flash()
    {
        if (!isFlashing)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sharedMaterials = whiteMaterials[i];
            }

            isFlashing = true;
        }

        LifetimeRunner.Register(this, flashDuration);
    }

    public void OnLifetimeExpired()
    {
        Restore();
    }

    public void Restore()
    {
        LifetimeRunner.Unregister(this);

        if (!isFlashing)
            return;

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterials = originalMaterials[i];
        }

        isFlashing = false;
    }

    private void OnDisable()
    {
        Restore();
    }
}
