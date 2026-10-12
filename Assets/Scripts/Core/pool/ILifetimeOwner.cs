public interface ILifetimeOwner
{
    int LifetimeIndex { get; set; }
    void OnLifetimeExpired();
}
