using wow_fishbot_sharp.Hooks;
using wow_fishbot_sharp.Utils;

namespace wow_fishbot_sharp
{
    class Program
    {
        private static readonly KeyboardHook KeyboardHook = new();
        private static readonly MouseHook MouseHook = new();
        private static readonly WinEventHook WinEventHook = new();
        private static readonly FishingStateMachine FishingStateMachine = new();

        private static async Task Main()
        {
            Console.WriteLine("[Main]: Started, pres DEL to stop");

            AudioUtils.SetVolumeToMax();
            AudioUtils.MuteSound();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task initMessageLoopAndHooks = Task.Run(() => { InitHooksAndMessageLoop(cancellationTokenSource.Token); });

            WinEventHook.OnCursorIconChange += (_, e) => { Console.WriteLine(e); FishingStateMachine.NotifyBobberFound(); };

            Task stateMachineTask = Task.Run(() =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    FishingStateMachine.Update(cancellationTokenSource.Token);
                    Thread.Sleep(10);
                }
            });

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
            uint processId = WindowUtils.GetProcessIdByWindowName("World of Warcraft");
            while (processId == 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("[Main]: Cancellation requested, exiting InitHooksAndMessageLoop");
                    return;
                }

                Console.WriteLine("[Main]: Failed to get processId, will retry!");
                Thread.Sleep(2000);
                processId = WindowUtils.GetProcessIdByWindowName("World of Warcraft");
            }

            Console.WriteLine($"[Main]: Got process ID: {processId}");
            WinEventHook.HookWinEvent(processId);

            // start message loop
            Application.Run();
        }
    }
}