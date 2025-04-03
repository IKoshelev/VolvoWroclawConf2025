namespace DemoPWA.Services;

public interface INeedInit
{
    public bool WasInit { get; }
    public Task Init();
}
