# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Common Development Commands

### Build and Run
```bash
# Build the project
dotnet build

# Run the application
dotnet run --project Phishy/Phishy.csproj

# Build in Release mode
dotnet build -c Release
```

### Common Issues
- The executable is named `guess.exe` (not Phishy.exe) for obfuscation purposes
- Build output is in `Phishy/bin/Debug/net7.0-windows/` or `Phishy/bin/Release/net7.0-windows/`

## Architecture Overview

Phishy is a Windows-only fishing automation tool for World of Warcraft that operates entirely out-of-process using Windows API hooks. The architecture follows clean SOLID principles with dependency injection interfaces.

### Core Components

1. **State Machine (FishingStateMachine.cs)**
   - Central logic implementing state pattern
   - States: Start, Login/Logout, ApplyLure, CastLine, FindBobber, WaitAndCatch, CatchFish
   - Thread-safe bobber detection with locking

2. **Hook System**
   - **WinEventHook**: Detects cursor changes to identify bobber (EVENT_OBJECT_NAMECHANGE)
   - **KeyboardHook**: Global keyboard input monitoring
   - **MouseHook**: Mouse event tracking
   
3. **Audio Detection**
   - Uses NAudio to monitor Windows audio peak levels
   - **CRITICAL**: Requires `SetupSound: true` in configuration.yaml
   - Must set volume to 100% and mute to detect fish splashes
   - Detection threshold is 0.1f (fine-tuned, do not change)

4. **Configuration**
   - YAML-based configuration with validation
   - Properties.cs defines all config options with defaults
   - ConfigValidator.cs ensures required fields are set

### Important Audio Setup Requirements

The audio detection system is critical for fish detection:

1. **SetupSound must be true** in configuration.yaml - this is the most common issue
2. When enabled, the bot will:
   - Set Windows volume to 100%
   - Mute the system audio
   - Monitor audio peaks through AudioMeterInformation API
3. The bot detects fish by monitoring audio levels while muted (peaks still register)
4. Current detection threshold is 0.1f and should not be changed

### Key Technical Details

- Targets .NET 9.0 Windows (net9.0-windows)
- Uses Windows Forms for message loop
- Reduced polling from 100Hz to 20Hz for CPU efficiency
- All Windows API calls include proper error handling with Win32 error codes
- Thread-safe operations with proper locking mechanisms
- Resource disposal for COM objects
- Exit key is END (not DEL)

### Interact Key Feature

The bot supports two fishing modes:

1. **Traditional Mode (default)**: Uses cursor detection + mouse clicks
   - Detects bobber via cursor icon changes (EVENT_OBJECT_NAMECHANGE)
   - Moves mouse in spiral pattern to find bobber
   - Right-clicks on bobber to catch fish

2. **Interact Key Mode**: Uses WoW's "Interact with Target" feature
   - Set `use-interact-key: true` in configuration.yaml
   - Configure `keyboard-key-interact` (default: f)
   - Skips bobber detection entirely
   - Uses keyboard interaction instead of mouse clicks
   - Requires WoW expansion that supports interact feature (not available in vanilla)

### Testing Audio Issues

If audio detection isn't working:
1. Check that `SetupSound: true` in configuration.yaml
2. Run the diagnostic logging to see audio device state
3. Ensure the game window name matches exactly in config
4. Make sure Windows audio service is running
5. The bot needs to run with proper Windows audio permissions

### Common Development Patterns

- Always check existing code style before making changes
- Use the existing interfaces for dependency injection
- Follow the established error handling patterns with Win32 error codes
- Maintain thread safety in shared state (see _bobberLock usage)
- Keep the state machine transitions clean and predictable

### Code Comments Philosophy

When writing or reviewing comments, follow these principles:

1. **Avoid redundant comments** that simply restate what the code does:
   - BAD: `// Set the mouse position` above `SetCursorPos(x, y)`
   - BAD: `// Track max sound level` above `if (current > max) max = current`
   - BAD: `// reduced from 10ms to save CPU` - historical changes without context are meaningless

2. **Keep comments that explain non-obvious concepts**:
   - GOOD: `// Calculate the number of iterations for the spiral` - explains the visual pattern created by math
   - GOOD: `// Log every 2 seconds or when significant sound detected` - explains complex conditional logic
   - GOOD: Comments explaining WHY something is done, not WHAT is done

3. **The code should speak for itself** - well-named variables and functions eliminate most need for comments

4. **Never include historical information** unless it provides critical context for current behavior