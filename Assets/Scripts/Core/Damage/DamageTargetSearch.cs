using System;
using System.Collections.Generic;
using UnityEngine;

public static class DamageTargetSearch
{
    private static Collider[] overlapBuffer = new Collider[256];

    public static Collider FindNearest(Vector3 center, float radius, TeamType targetTeam, out IDamageable target, List<Collider> exclude = null)
    {
        int count = OverlapSphere(center, radius);

        Collider nearest = null;
        float nearestSqrDistance = float.MaxValue;
        target = null;

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];

            if (exclude != null && exclude.Contains(col))
                continue;

            if (!DamageTargetRegistry.TryResolve(col, out IDamageable damageable))
                continue;

            if ((damageable.TeamType & targetTeam) == 0)
                continue;

            float sqrDistance = (col.transform.position - center).sqrMagnitude;

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearest = col;
                target = damageable;
            }
        }

        Array.Clear(overlapBuffer, 0, count);
        return nearest;
    }

    private static int OverlapSphere(Vector3 center, float radius)
    {
        int count = Physics.OverlapSphereNonAlloc(center, radius, overlapBuffer);

        while (count == overlapBuffer.Length)
        {
            overlapBuffer = new Collider[overlapBuffer.Length * 2];
            count = Physics.OverlapSphereNonAlloc(center, radius, overlapBuffer);
        }

        return count;
    }
}
