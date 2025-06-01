namespace Phishy.Interfaces;

public interface IHook : IDisposable
{
    void Install();
    void Uninstall();
    bool IsInstalled { get; }
}