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
                Console.WriteLine("[Main]: Audio setup enabled - configuring audio settings...");
                
                Console.WriteLine("[Main]: Audio state BEFORE configuration:");
                AudioUtils.LogAudioDeviceInfo();
                
                Console.WriteLine("[Main]: Setting win volume to max...");
                AudioUtils.SetVolumeToMax();
                
                Console.WriteLine("[Main]: Muting sound...");
                AudioUtils.MuteSound();
                
                Console.WriteLine("[Main]: Audio state AFTER configuration:");
                AudioUtils.LogAudioDeviceInfo();
            }
            else
            {
                Console.WriteLine("[Main]: Audio setup disabled in config (SetupSound = false)");
                Console.WriteLine("[Main]: Current audio state:");
                AudioUtils.LogAudioDeviceInfo();
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task initMessageLoopAndHooks = Task.Run(() => { InitHooksAndMessageLoop(cancellationTokenSource.Token); });

            WinEventHook.OnCursorIconChange += (_, e) => { Console.WriteLine('\n' + e); FishingStateMachine.NotifyBobberFound(); };

            Task stateMachineTask = Task.Run(() =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    FishingStateMachine.Update(cancellationTokenSource.Token);
                    Thread.Sleep(50);
                }
            });

            Console.WriteLine("[Main]: Started, press END to stop");
            while (Console.ReadKey().Key != ConsoleKey.End)
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

            Application.Run();
        }
    }
}