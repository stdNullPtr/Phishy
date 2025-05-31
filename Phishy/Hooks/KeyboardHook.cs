using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Phishy.Hooks
{
    internal sealed class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYUP = 0x105;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private readonly LowLevelKeyboardProc _proc;
        private IntPtr _hookId = IntPtr.Zero;

        public event EventHandler<Keys>? OnKeyPressed;
        public event EventHandler<Keys>? OnKeyReleased;

        public KeyboardHook()
        {
            _proc = HookCallback;
        }

        public void HookKeyboard()
        {
            _hookId = SetHook(_proc);
            if (_hookId == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                throw new InvalidOperationException($"Failed to install keyboard hook. Error: {error}");
            }
        }

        public void UnHookKeyboard()
        {
            if (_hookId != IntPtr.Zero)
            {
                if (!UnhookWindowsHookEx(_hookId))
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"[KeyboardHook]: Failed to unhook keyboard. Error: {error}");
                }
                _hookId = IntPtr.Zero;
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule? curModule = curProcess.MainModule;
            if (curModule == null)
            {
                Console.WriteLine("[KeyboardHook]: Failed to get current module");
                return IntPtr.Zero;
            }
            
            IntPtr moduleHandle = GetModuleHandle(curModule.ModuleName);
            if (moduleHandle == IntPtr.Zero)
            {
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine($"[KeyboardHook]: Failed to get module handle. Error: {error}");
                return IntPtr.Zero;
            }
            
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, moduleHandle, 0);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                OnKeyPressed?.Invoke(this, (Keys)vkCode);
            }
            else if (nCode >= 0 && wParam == WM_KEYUP || wParam == WM_SYSKEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                OnKeyReleased?.Invoke(this, (Keys)vkCode);
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
