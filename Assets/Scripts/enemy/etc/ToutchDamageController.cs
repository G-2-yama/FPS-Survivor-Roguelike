using UnityEngine;

/// <summary>敵の接触ダメージ量を提供する</summary>
public class ToutchDamageController : MonoBehaviour, IContactDamageSource
{
    [SerializeField, Min(0), PlayerSettingLabel("Damage Amount"), Tooltip("プレイヤーとの最初の接触時に与えるダメージ量")]
    private int damageAmount = 100;
    private float damageMultiplier = 1f;

    public int DamageAmount => EnemyStatScaling.Scale(damageAmount, damageMultiplier);

    public void SetDamageMultiplier(float multiplier) => damageMultiplier = multiplier;
}

