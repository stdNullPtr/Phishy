using System.Runtime.InteropServices;
using System.Text;

namespace wow_fishbot_sharp.Utils;

internal class WindowUtils
{
    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

    public static Point? GetWindowCenterPoint(string windowName)
    {
        IntPtr windowHandle = FindWindow(null, windowName);
        if (windowHandle == IntPtr.Zero)
        {
            Console.WriteLine("[WindowUtils]: Failed retrieving window handle");
            return null;
        }

        if (GetClientRect(windowHandle, out var clientRect))
        {
            Point centerPoint = default;
            centerPoint.X = (clientRect.Left + clientRect.Right) / 2;
            centerPoint.Y = (clientRect.Top + clientRect.Bottom) / 2;

            ClientToScreen(windowHandle, ref centerPoint);
            
            return centerPoint;
        }

        Console.WriteLine("[WindowUtils]: Failed GetClientRect");
        return null;
    }

    public static uint GetProcessIdByWindowName(string windowName)
    {
        IntPtr windowHandle = FindWindow(null, windowName);
        if (windowHandle == IntPtr.Zero)
        {
            Console.WriteLine("[WindowUtils]: Failed retrieving window handle");
            return 0;
        }

        GetWindowThreadProcessId(windowHandle, out uint processId);

        return processId;
    }

    public static string GetForegroundWindowName()
    {
        IntPtr foregroundWindowHandle = GetForegroundWindow();
        return GetWindowTitle(foregroundWindowHandle);
    }

    private static string GetWindowTitle(IntPtr windowHandle)
    {
        const int maxWindowTitleLength = 256;
        StringBuilder windowTitleBuilder = new StringBuilder(maxWindowTitleLength);
        int length = GetWindowText(windowHandle, windowTitleBuilder, maxWindowTitleLength);
        string windowTitle = windowTitleBuilder.ToString(0, length);

        return windowTitle;
    }

    #region structs

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion
}