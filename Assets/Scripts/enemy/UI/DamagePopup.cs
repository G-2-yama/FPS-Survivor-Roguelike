using UnityEngine;
using TMPro;

public class DamagePopup : PoolableObject
{
    [SerializeField] private TextMeshPro damageText;

    [SerializeField] private float lifeTime = 0.8f;
    [SerializeField] private float moveSpeed = 1.5f;

    private float timer;
    private int lastTickFrame = -1;

    internal int RunnerIndex { get; set; } = -1;

    public void Setup(
        Color color,
        int damage,
        Vector3 position,
        Transform player)
    {
        transform.position = position;

        damageText.text = damage.ToString();

        // Žó‚¯Žæ‚Á‚½F‚ðÝ’è
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

        timer = 0f;
        lastTickFrame = -1;
        DamagePopupRunner.Register(this);
        Tick();
    }

    public void Tick()
    {
        if (lastTickFrame == Time.frameCount)
            return;
        lastTickFrame = Time.frameCount;

        if (timer >= lifeTime)
        {
            Release();
            return;
        }

        timer += Time.deltaTime;

        transform.position +=
            Vector3.up * moveSpeed * Time.deltaTime;

        SetVertexAlpha(1f - timer / lifeTime);
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

            meshInfo.mesh.colors32 = colors;
        }
    }

    public override bool KeepActiveInPool => true;

    public override void OnGet()
    {
        Color color = damageText.color;
        color.a = 1f;
        damageText.color = color;
        damageText.renderer.enabled = true;
    }

    public override void OnRelease()
    {
        DamagePopupRunner.Unregister(this);
        damageText.renderer.enabled = false;
    }

    private void OnDisable()
    {
        DamagePopupRunner.Unregister(this);
    }
}