public interface IPoolable
{
    void OnGet();
    void OnRelease();
}
