using Phishy.Configs;
using Phishy.Hooks;
using Phishy.Utils;

namespace Phishy
{
    class Program
    {
        private static readonly KeyboardHook KeyboardHook = new();
        private static readonly MouseHook MouseHook = new();
        private static readonly WinEventHook WinEventHook = new();
        private static readonly FishingStateMachine FishingStateMachine = new();

        private static async Task Main()
        {
            Console.CursorVisible = false;

            Console.WriteLine("[Main]: Loading app properties");
            if (!AppConfig.LoadProperties())
            {
                Console.WriteLine("[Main]: Failed to load app properties, exiting...");
                return;
            }

            if (AppConfig.Props.SetupSound)
            {
                Console.WriteLine("[Main]: Setting win volume to max and mute");
                AudioUtils.SetVolumeToMax();
                AudioUtils.MuteSound();
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task initMessageLoopAndHooks = Task.Run(() => { InitHooksAndMessageLoop(cancellationTokenSource.Token); });

            WinEventHook.OnCursorIconChange += (_, e) => { Console.WriteLine('\n' + e); FishingStateMachine.NotifyBobberFound(); };

            Task stateMachineTask = Task.Run(() =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    FishingStateMachine.Update(cancellationTokenSource.Token);
                    // Reduced polling frequency from 100Hz to 20Hz to save CPU
                    Thread.Sleep(50);
                }
            });

            Console.WriteLine("[Main]: Started, pres DEL to stop");
            while (Console.ReadKey().Key != ConsoleKey.Delete)
            {
                Thread.Sleep(100);
            }

            cancellationTokenSource.Cancel();
            Application.Exit();
            Console.WriteLine("[Main]: Message loop stopped");
            await initMessageLoopAndHooks.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            await stateMachineTask.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

            Console.WriteLine("[Main]: Unhooking...");
            KeyboardHook.UnHookKeyboard();
            MouseHook.UnHookMouse();
            WinEventHook.UnHookWinEvent();
        }

        private static void InitHooksAndMessageLoop(CancellationToken cancellationToken)
        {
            uint processId = WindowUtils.GetProcessIdByWindowName(AppConfig.Props.GameWindowName);
            while (processId == 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("[Main]: Cancellation requested, exiting InitHooksAndMessageLoop");
                    return;
                }

                Console.WriteLine("[Main]: Failed to get processId, will retry!");
                Thread.Sleep(2000);
                processId = WindowUtils.GetProcessIdByWindowName(AppConfig.Props.GameWindowName);
            }

            Console.WriteLine($"[Main]: Got process ID: {processId}");
            WinEventHook.HookWinEvent(processId);

            // start message loop
            Application.Run();
        }
    }
}