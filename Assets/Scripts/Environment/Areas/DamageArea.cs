using UnityEngine;
using System.Collections.Generic;

public class DamageArea : MonoBehaviour
{
    [SerializeField] private Collider areaCollider;

    [SerializeField] private int damage = 10;
    [SerializeField] private float interval = 1f;

    /// <summary>
    /// 各対象ごとのダメージタイマーを管理する辞書
    /// </summary>
    private Dictionary<IDamageable, float> timers = new Dictionary<IDamageable, float>();

    /// <summary>
    /// 対象ごとに時間を加算し、一定間隔ごとにダメージを与える
    /// </summary>
    /// <param name="other">Trigger内に入っているコライダー</param>
    private void OnTriggerStay(Collider other)
    {
        var damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;

        // 初回登録
        if (!timers.ContainsKey(damageable))
        {
            timers[damageable] = 0f;
        }

        // タイマー加算
        timers[damageable] += Time.deltaTime;

        // 間隔を超えたらダメージ
        if (timers[damageable] >= interval)
        {
            damageable.TakeDamage(damage);
            timers[damageable] = 0f;
        }
    }

    /// <summary>
    /// Triggerから離れた際に対象のタイマーを削除する
    /// </summary>
    /// <param name="other">Triggerから離れたコライダー</param>
    private void OnTriggerExit(Collider other)
    {
        var damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;

        timers.Remove(damageable);
    }
}
