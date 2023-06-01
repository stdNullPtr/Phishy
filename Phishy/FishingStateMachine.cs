using Phishy.Configs;
using Phishy.Utils;

namespace Phishy;

public enum FishingState
{
    Start,
    ApplyLure,
    ApplySecondLure,
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
    private DateTime _lastSecondLureApplyTime;

    public FishingStateMachine()
    {
        _currentState = FishingState.Start;
        _lastLureApplyTime = DateTime.Now.AddDays(-69);
        _lastSecondLureApplyTime = DateTime.Now.AddDays(-69);
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
                    Thread.Sleep(1000);
                    break;
                }

                _isBobberDipped = _isBobberFound = _isLineCast = false;

                Console.WriteLine("[FishingStateMachine]: Moving to center of screen...");
                MouseUtils.MoveToCenterOfWindow(AppConfig.Props.GameWindowName, true, 100);

                TryTransition();
                break;
            case FishingState.ApplyLure:
                Console.WriteLine("[FishingStateMachine]: Applying lure...");

                ApplyLure();
                _lastLureApplyTime = DateTime.Now;

                TryTransition();
                break;
            case FishingState.ApplySecondLure:
                Console.WriteLine("[FishingStateMachine]: Applying second lure...");

                ApplySecondLure();
                _lastSecondLureApplyTime = DateTime.Now;

                TryTransition();
                break;
            case FishingState.CastLine:
                Console.WriteLine("[FishingStateMachine]: Casting the fishing line...");

                CastLine();
                _isLineCast = true;

                TryTransition();
                break;

            case FishingState.FindBobber:
                Console.WriteLine("[FishingStateMachine]: Looking for the bobber...");

                FindBobber(cancellationToken);

                TryTransition();
                break;
            case FishingState.WaitAndCatch:
                Console.WriteLine("[FishingStateMachine]: Waiting for a fish to bite...");
                ListenForFish(cancellationToken, TimeSpan.FromSeconds(AppConfig.Props.FishingChannelDurationSeconds));
                Console.WriteLine("[FishingStateMachine]: Stopped listening for fish.");

                TryTransition();
                break;
            case FishingState.CatchFish:
                Console.WriteLine("[FishingStateMachine]: You caught a fish!");

                MouseUtils.SendMouseInput(MouseButtons.Right);
                _isLineCast = false;
                Thread.Sleep(1000);

                TryTransition();
                break;
        }
    }

    private void TryTransition()
    {
        switch (_currentState)
        {
            case FishingState.Start:
                TransitionTo(DateTime.Now - _lastLureApplyTime > TimeSpan.FromMinutes(AppConfig.Props.LureBuffDurationMinutes)
                    ? FishingState.ApplyLure
                    : FishingState.CastLine);
                break;
            case FishingState.ApplyLure:
                if (AppConfig.Props.LureBuffSecondDurationMinutes.HasValue && AppConfig.Props.LureBuffSecondDurationMinutes > 0)
                {
                    if (DateTime.Now - _lastSecondLureApplyTime > TimeSpan.FromMinutes(AppConfig.Props.LureBuffSecondDurationMinutes.Value))
                    {
                        TransitionTo(FishingState.ApplySecondLure);
                        break;
                    }
                }

                TransitionTo(FishingState.CastLine);
                break;
            case FishingState.ApplySecondLure:
                TransitionTo(FishingState.CastLine);
                break;
            case FishingState.CastLine:
                if (_isLineCast)
                {
                    TransitionTo(FishingState.FindBobber);
                }
                break;
            case FishingState.FindBobber:
                // TODO: cover the case when line is cast but bobber is not found in time and make findBobber async
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
                if (_isBobberDipped)
                {
                    Console.WriteLine("[FishingStateMachine]: DIP!");
                    _isBobberDipped = false;
                    _isBobberFound = false;
                    TransitionTo(FishingState.CatchFish);
                }
                else
                {
                    TransitionTo(FishingState.Start);
                }
                break;
            case FishingState.CatchFish:
                TransitionTo(FishingState.Start);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(_currentState), _currentState, null);
        }
    }

    private void TransitionTo(FishingState state)
    {
        _currentState = state;
    }

    private void ApplySecondLure()
    {
        if (string.IsNullOrWhiteSpace(AppConfig.Props.KeyboardKeyApplySecondLure))
        {
            Console.WriteLine("[FishingStateMachine]: Can't apply second lure, invalid button configured.");
            return;
        }

        KeyboardUtils.SendKeyInput(AppConfig.Props.KeyboardKeyApplySecondLure);
        Thread.Sleep(3000);
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

            //TODO move this outside and check during tryTransition and possibly cancel through token
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