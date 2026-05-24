using System.Collections;
using UnityEngine;

public class WhiteFlash : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private Material whiteFlashMaterial;
    [SerializeField] private float flashDuration = 0.08f;

    private Material[][] originalMaterials;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].materials;
        }
    }

    public void Flash()
    {
        Restore();

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] whiteMaterials = new Material[renderers[i].materials.Length];

            for (int j = 0; j < whiteMaterials.Length; j++)
            {
                whiteMaterials[j] = whiteFlashMaterial;
            }

            renderers[i].materials = whiteMaterials;
        }

        yield return new WaitForSeconds(flashDuration);

        Restore();
    }

    public void Restore()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].materials = originalMaterials[i];
        }
    }

    private void OnDisable()
    {
        Restore();
    }
}
