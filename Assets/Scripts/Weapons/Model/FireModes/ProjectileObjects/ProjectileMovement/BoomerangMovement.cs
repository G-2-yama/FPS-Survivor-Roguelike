using UnityEngine;

public class BoomerangMovement : MovementBase
{
    [SerializeField] private float width = 4f;      // 横方向の大きさ
    [SerializeField] private float length = 8f;     // 前方への距離
    [SerializeField] private float duration = 1.2f; // 1周する時間
    private float timer;

    protected override void Update()
    {
        timer += Time.deltaTime;

        float t = timer / duration;

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