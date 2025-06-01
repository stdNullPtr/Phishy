using System.Runtime.InteropServices;

namespace Phishy.Hooks
{
    internal sealed class MouseHook
    {
        private const int WH_MOUSE_LL = 14;
        private const IntPtr WM_MOUSEMOVE = 0x0200;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private readonly LowLevelMouseProc _hookProcDelegate;
        private IntPtr _hookId = IntPtr.Zero;

        public event EventHandler<string>? OnCursorMove;

        public MouseHook()
        {
            _hookProcDelegate = HookCallback;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == WM_MOUSEMOVE)
                {
                    int x = Marshal.ReadInt32(lParam);
                    int y = Marshal.ReadInt32(lParam + 4);

                    OnCursorMove?.Invoke(this, $"[MouseHook]: Mouse moved to X:{x}, Y:{y}");
                }
                else
                {
                    Console.WriteLine($"[MouseHook]: DEBUG: {wParam:X}");
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void HookMouse()
        {
            _hookId = SetHook(_hookProcDelegate);
            if (_hookId == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to install mouse hook. Error: {error}");
            }
        }

        public void UnHookMouse()
        {
            if (_hookId != IntPtr.Zero)
            {
                if (!UnhookWindowsHookEx(_hookId))
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"[MouseHook]: Failed to unhook mouse. Error: {error}");
                }
                _hookId = IntPtr.Zero;
            }
        }

        private IntPtr SetHook(LowLevelMouseProc hookProcDelegate)
        {
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            if (curModule == null)
            {
                Console.WriteLine("[MouseHook]: Failed to get current module");
                return IntPtr.Zero;
            }
            
            IntPtr moduleHandle = GetModuleHandle(curModule.ModuleName);
            if (moduleHandle == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine($"[MouseHook]: Failed to get module handle. Error: {error}");
                return IntPtr.Zero;
            }
            
            return SetWindowsHookEx(WH_MOUSE_LL, hookProcDelegate, moduleHandle, 0);
        }
    }
}
