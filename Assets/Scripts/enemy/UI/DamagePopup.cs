using UnityEngine;
using TMPro;
using System.Collections;

public class DamagePopup : PoolableObject
{
    [SerializeField] private TextMeshPro damageText;

    [SerializeField] private float lifeTime = 0.8f;
    [SerializeField] private float moveSpeed = 1.5f;

    private Coroutine popupCoroutine;

    public void Setup(
        Color color,
        int damage,
        Vector3 position,
        Transform player)
    {
        transform.position = position;

        damageText.text = damage.ToString();

        // éÛÇØéÊÇ¡ÇΩêFÇê›íË
        color.a = 1f;
        damageText.color = color;
        damageText.ForceMeshUpdate();

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            transform.rotation =
                Quaternion.LookRotation(-direction);
        }

        popupCoroutine = StartCoroutine(PopupRoutine());
    }

    private IEnumerator PopupRoutine()
    {
        float timer = 0f;

        while (timer < lifeTime)
        {
            timer += Time.deltaTime;

            transform.position +=
                Vector3.up * moveSpeed * Time.deltaTime;

            SetVertexAlpha(1f - timer / lifeTime);

            yield return null;
        }

        Release();
    }

    private void SetVertexAlpha(float alpha)
    {
        byte a = (byte)Mathf.Round(Mathf.Clamp01(alpha) * 255f);
        TMP_TextInfo textInfo = damageText.textInfo;

        for (int i = 0; i < textInfo.materialCount; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
            Color32[] colors = meshInfo.colors32;

            for (int j = 0; j < meshInfo.vertexCount; j++)
            {
                colors[j].a = a;
            }
        }

        damageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    public override void OnGet()
    {
        Color color = damageText.color;
        color.a = 1f;
        damageText.color = color;
    }

    public override void OnRelease()
    {
        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
            popupCoroutine = null;
        }
    }
}