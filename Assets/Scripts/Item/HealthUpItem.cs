using UnityEngine;


/// <summary>プレイヤーの最大HPを強化し、必要に応じて解除するアイテム</summary>
[CreateAssetMenu(menuName = "Items/healthUp")]
public class HealthUpItem : Item
{
    [SerializeField, Min(0), PlayerSettingLabel("Health Increase Amount"), Tooltip("最大HPと現在HPに加算する値")]
    private int HealthIncreaseAmount = 200;

    /// <summary>体力強化の取得が可能か判定する</summary>
    public override bool IsAvailable()
    {
        return true;
    }

    /// <summary>最大HPを強化する</summary>
    public override void Apply()
    {
        player.Stats.AddMaxHP(HealthIncreaseAmount);
    }

    /// <summary>適用した最大HP強化を解除する</summary>
    public override void Revert()
    {
        player.Stats.AddMaxHP(-HealthIncreaseAmount);
    }
}
