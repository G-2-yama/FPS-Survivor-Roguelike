using UnityEngine;
using System.Collections.Generic;

public class DamageArea : DamageBase
{
    [SerializeField]
    private DamageAreaMode mode;

    [SerializeField]
    private float interval = 1f;

    Dictionary<IDamageable, float> timers = new();

    private void OnTriggerEnter(Collider other)
    {
        if (mode != DamageAreaMode.OnEnterOnce)
            return;

        TryDamage(other, out _);
    }

    private void OnTriggerStay(Collider other)
    {
        if (mode != DamageAreaMode.Interval)
            return;

        if (!other.TryGetComponent(out IDamageable damageable))
            return;

        if ((damageable.TeamType & targetTeam) == 0)
            return;

        if (!timers.ContainsKey(damageable))
            timers[damageable] = 0;

        timers[damageable] += Time.deltaTime;

        if (timers[damageable] >= interval)
        {
            damageable.TakeDamage(damage, knockbackForce);
            timers[damageable] = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out IDamageable damageable))
            return;

        timers.Remove(damageable);
    }

    public override void OnRelease()
    {
        timers.Clear();
        base.OnRelease();
    }
}

public enum DamageAreaMode
{
    OnEnterOnce, // 入った瞬間に一度だけダメージ
    Interval,      // 入っている間、一定間隔でダメージ
}