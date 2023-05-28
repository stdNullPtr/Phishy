using System.Runtime.InteropServices;
using static System.Windows.Forms.AxHost;

namespace wow_fishbot_sharp.Utils;

internal class MouseUtils
{
    private const int INPUT_MOUSE = 0;
    private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const int MOUSEEVENTF_RIGHTUP = 0x0010;
    private const int MOUSEEVENTF_LEFTDOWN = 0x0008;
    private const int MOUSEEVENTF_LEFTUP = 0x0010;

    [DllImport("user32.dll")]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out Point lpPoint);

    public static void SendMouseInput(MouseButtons button)
    {
        INPUT mouseInput = new INPUT
        {
            Type = INPUT_MOUSE
        };

        uint pressedFlags;
        uint releaseFlags;

        switch (button)
        {
            case MouseButtons.Left:
                pressedFlags = MOUSEEVENTF_LEFTDOWN;
                releaseFlags = MOUSEEVENTF_LEFTUP;
                break;
            case MouseButtons.Right:
                pressedFlags = MOUSEEVENTF_RIGHTDOWN;
                releaseFlags = MOUSEEVENTF_RIGHTUP;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(button), button, null);
        }

        mouseInput.Data.Mouse.Flags = pressedFlags;
        SendInput(1, new[] { mouseInput }, INPUT.Size);

        Thread.Sleep(50); // Delay between mouse events (adjust as needed)

        // Release the mouse button
        mouseInput.Data.Mouse.Flags = releaseFlags;
        SendInput(1, new[] { mouseInput }, INPUT.Size);
    }

    private static void MoveCursor(Point targetPoint)
    {
        SetCursorPos(targetPoint.X, targetPoint.Y);
    }

    private static void MoveCursor(Point targetPoint, bool smoothly)
    {
        if (smoothly)
        {
            GetCursorPos(out var cursorPosition);
            int startX = cursorPosition.X;
            int startY = cursorPosition.Y;
            int targetX = targetPoint.X;
            int targetY = targetPoint.Y;
            const int steps = 100;     // Number of steps for interpolation
            const int delay = 10;      // Delay between each step in milliseconds

            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                int x = Interpolate(startX, targetX, t);
                int y = Interpolate(startY, targetY, t);

                MoveCursor(new Point { X = x, Y = y });

                Thread.Sleep(delay);
            }
        }
        else
        {
            MoveCursor(targetPoint);
        }
    }

    private static int Interpolate(int start, int target, double t)
    {
        return (int)Math.Round(start + (target - start) * t);
    }

    public static void MoveToCenterOfWindow(string windowName, bool smoothly)
    {
        string foregroundWindowName = WindowUtils.GetForegroundWindowName();
        if (foregroundWindowName != windowName)
        {
            Console.WriteLine($"[MouseUtils]: Foreground window name is {foregroundWindowName} and not {windowName}, not moving mouse.");
            return;
        }

        Point centerPoint = WindowUtils.GetWindowCenterPoint(windowName)!.Value;

        MoveCursor(centerPoint, smoothly);
    }
    public static void MoveMouseFibonacci(CancellationToken cancellationToken, string windowName, ref bool isBobberAlreadyFound)
    {
        Point startingPoint = WindowUtils.GetWindowCenterPoint(windowName)!.Value;

        MoveToCenterOfWindow(windowName, true);

        // Radius and angular speed for the spiral
        double radius = 1.0;
        double angle = 0.1;
        const double angularSpeed = 0.1;
        const double radiusMod = 0.05;

        // Calculate the number of iterations for the spiral
        const int iterations = 600;

        for (int i = 0; i < iterations; i++)
        {
            if (cancellationToken.IsCancellationRequested || isBobberAlreadyFound)
            {
                break;
            }
            
            angle += angularSpeed;
            radius += radiusMod; 

            // Calculate the new mouse position
            startingPoint.X += (int)(radius * Math.Cos(angle));
            startingPoint.Y += (int)(radius * Math.Sin(angle));

            // Set the mouse position
            SetCursorPos(startingPoint.X, startingPoint.Y);

            // Sleep to control the speed of movement
            Thread.Sleep(10);
        }
    }

    #region structs
    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint Type;
        public MOUSEKEYBDHARDWAREINPUT Data;
        public static int Size => Marshal.SizeOf(typeof(INPUT));
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;
        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;
        [FieldOffset(0)]
        public HARDWAREINPUT Hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort KeyCode;
        public ushort Scan;
        public uint Flags;
        public uint Time;
        public IntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        public uint Msg;
        public ushort ParamL;
        public ushort ParamH;
    }
    #endregion
}