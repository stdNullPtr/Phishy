namespace Phishy.Interfaces;

public interface IWindowManager
{
    Point? GetWindowCenterPoint(string windowName);
    uint GetProcessIdByWindowName(string windowName);
    string GetForegroundWindowName();
}