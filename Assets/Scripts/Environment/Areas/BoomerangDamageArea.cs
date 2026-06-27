using UnityEngine;

public class BoomerangDamageArea : DamageArea
{
    private Transform owner;

    private Vector3 direction;

    [SerializeField] private float width = 4f;      // 横方向の大きさ
    [SerializeField] private float length = 8f;     // 前方への距離
    [SerializeField] private float duration = 1.2f; // 1周する時間

    private float timer;

    public void Initialize(int damage, float knockbackForce, Transform owner, Vector3 direction)
    {
        base.Initialize(damage, knockbackForce);

        this.owner = owner;
        this.direction = direction.normalized;

        timer = 0f;
    }

    private void Update()
    {
        if (owner == null)
        {
            Release();
            return;
        }

        timer += Time.deltaTime;

        float t = timer / duration;

        if (t >= 1f)
        {
            Release();
            return;
        }

        // プレイヤーの正面方向
        Vector3 forward = direction;

        // 正面に対して右方向
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        // 楕円軌道
        float x = Mathf.Sin(t * Mathf.PI * 2f) * width;
        float z = Mathf.Sin(t * Mathf.PI) * length;

        transform.position = owner.position + forward * z + right * x;
    }
}