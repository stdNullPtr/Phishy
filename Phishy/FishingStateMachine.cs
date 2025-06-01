using Phishy.Configs;
using Phishy.Interfaces;
using Phishy.Utils;

namespace Phishy;

public enum FishingState
{
    Start,
    Logout,
    Login,
    WaitForWintergrasp,
    ApplyLure,
    ApplySecondLure,
    CastLine,
    FindBobber,
    WaitAndCatch,
    CatchFish
}

public class FishingStateMachine : IFishingStateMachine
{
    private FishingState _currentState;
    private bool _isLineCast;
    private bool _isBobberDipped;
    private bool _isBobberFound;
    private readonly object _bobberLock = new object();

    private DateTime _lastLureApplyTime;
    private DateTime _lastApplyTimeSecondLure;

    public FishingStateMachine()
    {
        _currentState = FishingState.Start;
        _lastLureApplyTime = DateTime.Now.AddDays(-69);
        _lastApplyTimeSecondLure = DateTime.Now.AddDays(-69);
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
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    break;
                }

                _isBobberDipped = _isLineCast = false;
                lock (_bobberLock)
                {
                    _isBobberFound = false;
                }

                Console.WriteLine("[FishingStateMachine]: Moving to center of screen...");
                MouseUtils.MoveToCenterOfWindow(AppConfig.Props.GameWindowName, true, 100);

                TryTransition();
                break;
            case FishingState.Logout:
                Console.WriteLine("[FishingStateMachine]: Logging out...");
                Logout();
                TryTransition();
                break;
            case FishingState.WaitForWintergrasp:
                if (IsWintergraspRunning())
                {
                    Console.WriteLine("[FishingStateMachine]: Wintergrasp is running... sleeping 10 secs");
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                    break;
                }
                Console.WriteLine("[FishingStateMachine]: Wintergrasp is no longer running, resuming...");

                TryTransition();
                break;
            case FishingState.Login:
                Console.WriteLine("[FishingStateMachine]: Logging in...");
                Login();
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
                _lastApplyTimeSecondLure = DateTime.Now;

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

                if (AppConfig.Props.UseInteractKey)
                {
                    Console.WriteLine($"[FishingStateMachine]: Using interact key: {AppConfig.Props.KeyboardKeyInteract}");
                    KeyboardUtils.SendKeyInput(AppConfig.Props.KeyboardKeyInteract);
                }
                else
                {
                    MouseUtils.SendMouseInput(MouseButtons.Right);
                }
                _isLineCast = false;
                Thread.Sleep(TimeSpan.FromSeconds(1));

                TryTransition();
                break;
        }
    }

    private void TryTransition()
    {
        switch (_currentState)
        {
            case FishingState.Start:
                FishingState newState;

                if (AppConfig.Props.WaitForWintergrasp && (IsWintergraspAboutToBegin() || IsWintergraspRunning()))
                {
                    newState = FishingState.Logout;
                }
                else if (!string.IsNullOrWhiteSpace(AppConfig.Props.KeyboardKeyApplyLure) 
                && (DateTime.Now - _lastLureApplyTime > TimeSpan.FromMinutes(AppConfig.Props.LureBuffDurationMinutes)))
                    newState = FishingState.ApplyLure;
                else
                    newState = FishingState.CastLine;

                TransitionTo(newState);
                break;
            case FishingState.Logout:
                TransitionTo(FishingState.WaitForWintergrasp);
                break;
            case FishingState.WaitForWintergrasp:
                TransitionTo(FishingState.Login);
                break;
            case FishingState.Login:
                TransitionTo(FishingState.Start);
                break;
            case FishingState.ApplyLure:
                if (AppConfig.Props.SecondLureBuffDurationMinutes.HasValue && AppConfig.Props.SecondLureBuffDurationMinutes > 0)
                {
                    if (DateTime.Now - _lastApplyTimeSecondLure > TimeSpan.FromMinutes(AppConfig.Props.SecondLureBuffDurationMinutes.Value))
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
                    if (AppConfig.Props.UseInteractKey)
                    {
                        TransitionTo(FishingState.WaitAndCatch);
                    }
                    else
                    {
                        TransitionTo(FishingState.FindBobber);
                    }
                }
                break;
            case FishingState.FindBobber:
                // TODO: cover the case when line is cast but bobber is not found in time and make findBobber async
                bool bobberFound;
                lock (_bobberLock)
                {
                    bobberFound = _isBobberFound;
                }
                
                if (bobberFound && _isLineCast)
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
                    lock (_bobberLock)
                    {
                        _isBobberFound = false;
                    }
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
        Thread.Sleep(TimeSpan.FromSeconds(3));
    }

    private void ApplyLure()
    {
        KeyboardUtils.SendKeyInput(AppConfig.Props.KeyboardKeyApplyLure);
        Thread.Sleep(TimeSpan.FromSeconds(3));
    }

    private void CastLine()
    {
        KeyboardUtils.SendKeyInput(AppConfig.Props.KeyboardKeyStartFishing);
    }

    private void FindBobber(CancellationToken cancellationToken)
    {
        MouseUtils.MoveMouseFibonacci(cancellationToken, AppConfig.Props.GameWindowName, () =>
        {
            lock (_bobberLock)
            {
                return _isBobberFound;
            }
        });
    }

    private void ListenForFish(CancellationToken cancellationToken, TimeSpan timeoutInSeconds)
    {
        DateTime lastLineCastTime = DateTime.Now;
        DateTime lastLogTime = DateTime.Now;
        int checkCount = 0;
        float maxSoundLevel = 0f;
        const float FISH_DETECTION_THRESHOLD = 0.1f;
        
        string mode = AppConfig.Props.UseInteractKey ? "interact key" : "mouse click";
        Console.WriteLine($"[FishingStateMachine]: Starting to listen for fish splash (threshold: {FISH_DETECTION_THRESHOLD}, mode: {mode})...");
        
        while (!cancellationToken.IsCancellationRequested)
        {
            bool bobberFound;
            lock (_bobberLock)
            {
                bobberFound = _isBobberFound;
            }
            
            float currentSoundLevel = AudioUtils.GetMasterVolumeLevel();
            checkCount++;
            
            if (currentSoundLevel > maxSoundLevel)
            {
                maxSoundLevel = currentSoundLevel;
            }
            
            // Log every 2 seconds or when significant sound detected
            if (DateTime.Now - lastLogTime > TimeSpan.FromSeconds(2) || currentSoundLevel > 0.01f)
            {
                string statusMsg = AppConfig.Props.UseInteractKey 
                    ? $"Interact mode: Ready, Current sound: {currentSoundLevel:F4}, Max sound: {maxSoundLevel:F4}, Checks: {checkCount}"
                    : $"Bobber found: {bobberFound}, Current sound: {currentSoundLevel:F4}, Max sound: {maxSoundLevel:F4}, Checks: {checkCount}";
                Console.WriteLine($"[FishingStateMachine]: Listening... {statusMsg}");
                lastLogTime = DateTime.Now;
            }
            
            bool canDetectFish = AppConfig.Props.UseInteractKey || bobberFound;
            
            if (canDetectFish && currentSoundLevel > FISH_DETECTION_THRESHOLD)
            {
                Console.WriteLine($"[FishingStateMachine]: FISH DETECTED! Sound level: {currentSoundLevel:F4} (threshold: {FISH_DETECTION_THRESHOLD})");
                _isBobberDipped = true;
                break;
            }

            //TODO move this outside and check during tryTransition and possibly cancel through token
            if (DateTime.Now - lastLineCastTime > timeoutInSeconds)
            {
                Console.WriteLine($"[FishingStateMachine]: Timeout after {timeoutInSeconds.TotalSeconds} seconds! Max sound detected: {maxSoundLevel:F4}");
                break;
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }

    public void NotifyBobberFound()
    {
        lock (_bobberLock)
        {
            if (_isLineCast)
            {
                _isBobberFound = true;
            }
        }
    }

    public static bool IsWintergraspAboutToBegin()
    {
        TimeSpan currentTimeOfDay = DateTime.Now.TimeOfDay;

        for (int hour = 0; hour < 24; hour += 3)
        {
            TimeSpan wgStartTime = TimeSpan.FromHours(hour + 1);
            TimeSpan fewMinutesBeforeWgStart = wgStartTime.Subtract(TimeSpan.FromMinutes(5));

            if (fewMinutesBeforeWgStart < TimeSpan.Zero)
            {
                fewMinutesBeforeWgStart = TimeSpan.Zero;
            }

            if (currentTimeOfDay >= fewMinutesBeforeWgStart && currentTimeOfDay <= wgStartTime)
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsWintergraspRunning()
    {
        TimeSpan currentTimeOfDay = DateTime.Now.TimeOfDay;

        for (int hour = 0; hour < 24; hour += 3)
        {
            TimeSpan wgStartTime = TimeSpan.FromHours(hour + 1);
            TimeSpan wgEndTime = wgStartTime + TimeSpan.FromMinutes(30);

            // Check 10 minutes before start and 10 minutes after end just to be sure we won't be kicked from WG
            if (currentTimeOfDay - TimeSpan.FromMinutes(10) >= wgStartTime && currentTimeOfDay <= wgEndTime + TimeSpan.FromMinutes(10))
            {
                return true;
            }
        }

        
        return false;
    }

    private void Logout()
    {
        if (string.IsNullOrWhiteSpace(AppConfig.Props.KeyboardPressLogout))
        {
            Console.WriteLine("[FishingStateMachine]: Can't logout, invalid button configured.");
            return;
        }

        KeyboardUtils.SendKeyInput(AppConfig.Props.KeyboardPressLogout);
        Thread.Sleep(TimeSpan.FromSeconds(3));
    }
    private void Login()
    {
        KeyboardUtils.SendKeyInput("ENTER");
        Thread.Sleep(TimeSpan.FromSeconds(10));
    }
}