using UnityEngine;

public class OrbitDamageArea : DamageArea
{
    private Transform center;
    private float radius;
    private float speed;
    private float angle;

    public void Initialize(int damage, float knockbackForce, Transform center, float radius, float speed, float angle = 0f)
    {
        base.Initialize(damage, knockbackForce);

        this.center = center;
        this.radius = radius;
        this.speed = speed;
        this.angle = angle;
    }

    protected virtual void Update()
    {
        if (center == null)
            return;

        angle += speed * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3( Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;

        transform.position = center.position + offset;
        
        transform.rotation = Quaternion.LookRotation(offset.normalized, Vector3.up);
    }
}