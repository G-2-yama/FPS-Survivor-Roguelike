using UnityEngine;

public class BoomerangDamageArea : DamageArea
{
    private Transform owner;

    private Vector3 direction;
    private float speed;
    private float maxDistance;

    private Vector3 startPosition;

    private bool returning;

    public void Initialize(int damage, Transform owner, Vector3 direction, float speed, float maxDistance)
    {
        base.Initialize(damage);

        this.owner = owner;
        this.direction = direction.normalized;
        this.speed = speed;
        this.maxDistance = maxDistance;

        startPosition = owner.position;
        returning = false;

        transform.position = startPosition;
    }

    private void Update()
    {
        if (owner == null)
        {
            Release();
            return;
        }

        if (!returning)
        {
            // 前進
            transform.position += direction * speed * Time.deltaTime;

            float distance = Vector3.Distance(startPosition, transform.position);

            if (distance >= maxDistance)
            {
                returning = true;
            }
        }
        else
        {
            // プレイヤーへ戻る
            Vector3 returnDir = (owner.position - transform.position).normalized;

            transform.position += returnDir * speed * Time.deltaTime;

            // プレイヤーに戻ったら消える
            if (Vector3.Distance(transform.position, owner.position) < 0.5f)
            {
                Release();
            }
        }
    }
}