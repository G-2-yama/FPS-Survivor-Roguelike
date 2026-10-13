/// <summary>プレイヤー接触時に与えるダメージを提供する契約</summary>
public interface IContactDamageSource
{
    int DamageAmount { get; }
}
