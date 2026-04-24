using UnityEngine;

public class NormalProjectile : ProjectileObject
{
    protected override void HandleHit(Collider col)
    {
        if (onHit == null) return;

        onHit.Invoke(col);

        Release();
    }
}