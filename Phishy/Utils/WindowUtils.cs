using System.Runtime.InteropServices;
using System.Text;

namespace Phishy.Utils;

internal class WindowUtils
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

    public static Point? GetWindowCenterPoint(string windowName)
    {
        IntPtr windowHandle = FindWindow(null, windowName);
        if (windowHandle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            Console.WriteLine($"[WindowUtils]: Failed retrieving window handle for '{windowName}'. Error: {error}");
            return null;
        }

        if (GetClientRect(windowHandle, out var clientRect))
        {
            Point centerPoint = default;
            centerPoint.X = (clientRect.Left + clientRect.Right) / 2;
            centerPoint.Y = (clientRect.Top + clientRect.Bottom) / 2;

            if (!ClientToScreen(windowHandle, ref centerPoint))
            {
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine($"[WindowUtils]: Failed ClientToScreen. Error: {error}");
                return null;
            }

            return centerPoint;
        }

        int error = Marshal.GetLastWin32Error();
        Console.WriteLine($"[WindowUtils]: Failed GetClientRect. Error: {error}");
        return null;
    }

    public static uint GetProcessIdByWindowName(string windowName)
    {
        IntPtr windowHandle = FindWindow(null, windowName);
        if (windowHandle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            Console.WriteLine($"[WindowUtils]: Failed retrieving window handle for '{windowName}'. Error: {error}");
            return 0;
        }

        uint threadId = GetWindowThreadProcessId(windowHandle, out uint processId);
        if (threadId == 0)
        {
            int error = Marshal.GetLastWin32Error();
            Console.WriteLine($"[WindowUtils]: Failed GetWindowThreadProcessId. Error: {error}");
            return 0;
        }

        return processId;
    }

    public static string GetForegroundWindowName()
    {
        IntPtr foregroundWindowHandle = GetForegroundWindow();
        if (foregroundWindowHandle == IntPtr.Zero)
        {
            int error = Marshal.GetLastWin32Error();
            Console.WriteLine($"[WindowUtils]: Failed GetForegroundWindow. Error: {error}");
            return string.Empty;
        }
        return GetWindowTitle(foregroundWindowHandle);
    }

    private static string GetWindowTitle(IntPtr windowHandle)
    {
        const int maxWindowTitleLength = 256;
        StringBuilder windowTitleBuilder = new StringBuilder(maxWindowTitleLength);
        int length = GetWindowText(windowHandle, windowTitleBuilder, maxWindowTitleLength);
        
        if (length == 0)
        {
            int error = Marshal.GetLastWin32Error();
            if (error != 0) // 0 means no error, just empty title
            {
                Console.WriteLine($"[WindowUtils]: Failed GetWindowText. Error: {error}");
            }
            return string.Empty;
        }
        
        return windowTitleBuilder.ToString(0, length);
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