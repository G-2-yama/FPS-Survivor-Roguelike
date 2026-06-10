using UnityEngine;

public class OrbitDamageArea : DamageArea
{
    private Transform center;
    private float radius;
    private float speed;
    private float angle;

    public void Initialize(int damage, Transform center, float radius, float speed)
    {
        base.Initialize(damage);

        this.center = center;
        this.radius = radius;
        this.speed = speed;
    }

    protected virtual void Update()
    {
        if (center == null)
            return;

        angle += speed * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3( Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;

        transform.position = center.position + offset;
    }
}