using UnityEngine;

public class DamageObject : DamageBase
{
    private void OnTriggerEnter(Collider other)
    {
        if (isReleased)
            return;

        if (TryDamage(other, out _))
        {
            Release();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isReleased)
            return;

        if (TryDamage(collision.collider, out _))
        {
            Release();
        }
        else
        {
            // 壁などでも消えるなら
            Release();
        }
    }
}