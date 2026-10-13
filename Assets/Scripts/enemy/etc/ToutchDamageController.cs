using UnityEngine;

/// <summary>敵の接触ダメージ量を提供する</summary>
public class ToutchDamageController : MonoBehaviour, IContactDamageSource
{
    [SerializeField, Min(0), PlayerSettingLabel("Damage Amount"), Tooltip("プレイヤーとの最初の接触時に与えるダメージ量")]
    private int damageAmount = 100;

    public int DamageAmount => damageAmount;
}

