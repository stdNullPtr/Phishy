using Phishy.Interfaces;
using Phishy.Utils;

namespace Phishy.Services;

public class WindowManager : IWindowManager
{
    public Point? GetWindowCenterPoint(string windowName)
    {
        return WindowUtils.GetWindowCenterPoint(windowName);
    }

    public uint GetProcessIdByWindowName(string windowName)
    {
        return WindowUtils.GetProcessIdByWindowName(windowName);
    }

    public string GetForegroundWindowName()
    {
        return WindowUtils.GetForegroundWindowName();
    }
}