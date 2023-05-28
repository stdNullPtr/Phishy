using wow_fishbot_sharp.Utils;

namespace wow_fishbot_sharp;

public enum FishingState
{
    Start,
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

    public FishingStateMachine()
    {
        _currentState = FishingState.Start;
    }

    public void Update(CancellationToken cancellationToken)
    {
        switch (_currentState)
        {
            case FishingState.Start:
                if (WindowUtils.GetForegroundWindowName() != "World of Warcraft")
                {
                    Console.WriteLine("[FishingStateMachine]: waiting for wow window...");
                    Thread.Sleep(1000);
                    break;
                }

                _isBobberDipped = _isBobberFound = _isLineCast = false;

                Console.WriteLine("[FishingStateMachine]: Moving to center of screen...");
                MouseUtils.MoveToCenterOfWindow("World of Warcraft", true);

                _currentState = FishingState.CastLine;
                break;

            case FishingState.CastLine:
                Console.WriteLine("[FishingStateMachine]: Casting the fishing line...");

                CastLine();
                _isLineCast = true;
                Thread.Sleep(1000);

                _currentState = FishingState.FindBobber;
                break;

            case FishingState.FindBobber:
                Console.WriteLine("[FishingStateMachine]: Looking for the bobber...");

                FindBobber(cancellationToken);

                if (_isBobberFound && _isLineCast)
                {
                    _currentState = FishingState.WaitAndCatch;
                }
                else
                {
                    _currentState = FishingState.Start;
                }
                break;

            case FishingState.WaitAndCatch:
                Console.WriteLine("[FishingStateMachine]: Waiting for a fish to bite...");
                ListenForFish(cancellationToken, TimeSpan.FromSeconds(10));
                Console.WriteLine("[FishingStateMachine]: Stopped listening for fish.");

                if (_isBobberDipped)
                {
                    Console.WriteLine("[FishingStateMachine]: DIP!");
                    _isBobberDipped = false;
                    Console.WriteLine("[FishingStateMachine]: Resetting IsBobberFound.");
                    _isBobberFound = false;
                    _currentState = FishingState.CatchFish;
                }
                else
                {
                    _currentState = FishingState.Start;
                }

                break;

            case FishingState.CatchFish:
                Console.WriteLine("[FishingStateMachine]: You caught a fish!");

                MouseUtils.SendMouseInput(MouseButtons.Right);
                _isLineCast = false;
                Thread.Sleep(1000);

                _currentState = FishingState.Start;
                break;
        }
    }
    private void CastLine()
    {
        KeyboardUtils.SendKeyInput(Keys.D1);
    }

    private void FindBobber(CancellationToken cancellationToken)
    {
        MouseUtils.MoveMouseFibonacci(cancellationToken, "World of Warcraft", ref _isBobberFound);
    }

    private void ListenForFish(CancellationToken cancellationToken, TimeSpan timeoutInSeconds)
    {
        DateTime lastLineCastTime = DateTime.Now;
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_isBobberFound && AudioUtils.GetMasterVolumeLevel() > 0.02f)
            {
                Console.WriteLine("[FishingStateMachine]: Probably a fish!");
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