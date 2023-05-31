using Phishy.Configs;
using Phishy.Utils;

namespace Phishy;

public enum FishingState
{
    Start,
    ApplyLure,
    CastLine,
    FindBobber,
    WaitAndCatch,
    CatchFish
}

public class FishingStateMachine
{
    private FishingState _currentState;
    private bool _isLineCast;
    private bool _isBobberDipped;
    // todo thread unsafe
    private bool _isBobberFound;

    private DateTime _lastLureApplyTime;

    public FishingStateMachine()
    {
        _currentState = FishingState.Start;
        _lastLureApplyTime = DateTime.Now.AddDays(-69);
    }

    public void Update(CancellationToken cancellationToken)
    {
        switch (_currentState)
        {
            case FishingState.Start:
                //TODO add this logic to a TryTransition method and every state should check this
                if (WindowUtils.GetForegroundWindowName() != AppConfig.Props.GameWindowName)
                {
                    Console.WriteLine($"[FishingStateMachine]: Open game window [{AppConfig.Props.GameWindowName}]...");
                    Console.SetCursorPosition(Console.GetCursorPosition().Left, Console.GetCursorPosition().Top - 1);
                    Thread.Sleep(1000);
                    break;
                }
                Console.WriteLine();

                _isBobberDipped = _isBobberFound = _isLineCast = false;

                Console.WriteLine("[FishingStateMachine]: Moving to center of screen...");
                MouseUtils.MoveToCenterOfWindow(AppConfig.Props.GameWindowName, true, 100);

                TransitionTo(DateTime.Now - _lastLureApplyTime > TimeSpan.FromMinutes(AppConfig.Props.LureBuffDurationMinutes)
                    ? FishingState.ApplyLure
                    : FishingState.CastLine);

                break;
            case FishingState.ApplyLure:
                Console.WriteLine("[FishingStateMachine]: Applying lure...");

                ApplyLure();
                _lastLureApplyTime = DateTime.Now;

                TransitionTo(FishingState.CastLine);
                break;
            case FishingState.CastLine:
                Console.WriteLine("[FishingStateMachine]: Casting the fishing line...");

                CastLine();
                _isLineCast = true;

                TransitionTo(FishingState.FindBobber);
                break;

            case FishingState.FindBobber:
                Console.WriteLine("[FishingStateMachine]: Looking for the bobber...");

                FindBobber(cancellationToken);

                if (_isBobberFound && _isLineCast)
                {
                    TransitionTo(FishingState.WaitAndCatch);
                }
                else
                {
                    TransitionTo(FishingState.Start);
                }
                break;

            case FishingState.WaitAndCatch:
                Console.WriteLine("[FishingStateMachine]: Waiting for a fish to bite...");
                ListenForFish(cancellationToken, TimeSpan.FromSeconds(AppConfig.Props.FishingChannelDurationSeconds));
                Console.WriteLine("[FishingStateMachine]: Stopped listening for fish.");

                if (_isBobberDipped)
                {
                    Console.WriteLine("[FishingStateMachine]: DIP!");
                    _isBobberDipped = false;
                    Console.WriteLine("[FishingStateMachine]: Resetting IsBobberFound.");
                    _isBobberFound = false;
                    TransitionTo(FishingState.CatchFish);
                }
                else
                {
                    TransitionTo(FishingState.Start);
                }

                break;

            case FishingState.CatchFish:
                Console.WriteLine("[FishingStateMachine]: You caught a fish!");

                MouseUtils.SendMouseInput(MouseButtons.Right);
                _isLineCast = false;
                Thread.Sleep(1000);

                TransitionTo(FishingState.Start);
                break;
        }
    }

    private void TransitionTo(FishingState state)
    {
        _currentState = state;
    }

    private void ApplyLure()
    {
        KeyboardUtils.SendKeyInput(AppConfig.Props.KeyboardKeyApplyLure);
        Thread.Sleep(3000);
    }

    private void CastLine()
    {
        KeyboardUtils.SendKeyInput(AppConfig.Props.KeyboardKeyStartFishing);
    }

    private void FindBobber(CancellationToken cancellationToken)
    {
        MouseUtils.MoveMouseFibonacci(cancellationToken, AppConfig.Props.GameWindowName, ref _isBobberFound);
    }

    // go max volume on both win and wow, and mute
    private void ListenForFish(CancellationToken cancellationToken, TimeSpan timeoutInSeconds)
    {
        DateTime lastLineCastTime = DateTime.Now;
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_isBobberFound && AudioUtils.GetMasterVolumeLevel() > 0.1f)
            {
                Console.WriteLine($"[FishingStateMachine]: Probably a fish! (sound level: {AudioUtils.GetMasterVolumeLevel()})");
                _isBobberDipped = true;
                break;
            }

            if (DateTime.Now - lastLineCastTime > timeoutInSeconds)
            {
                Console.WriteLine($"[FishingStateMachine]: Did not find a fish in {timeoutInSeconds.TotalSeconds} seconds!");
                break;
            }

            Thread.Sleep(100);
        }
    }

    public void NotifyBobberFound()
    {
        if (_isLineCast)
        {
            _isBobberFound = true;
        }
    }
}