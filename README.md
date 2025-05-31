<div align="center">
  <h1>Phishy</h1>
  
  <p align="center">
    <strong>⚠️ USE AT YOUR OWN RISK ⚠️</strong>
  </p>

  <p align="center">
    An advanced out-of-process fishing automation tool for World of Warcraft using Windows API hooks
    <br />
    <br />
    <a href="#features">Features</a>
    ·
    <a href="#how-it-works">How It Works</a>
    ·
    <a href="#getting-started">Getting Started</a>
    ·
    <a href="#configuration">Configuration</a>
    <br />
    <br />
    <a href="https://github.com/stdNullPtr/Phishy/issues">Report Bug</a>
    ·
    <a href="https://github.com/stdNullPtr/Phishy/issues">Request Feature</a>
    ·
    <a href="https://github.com/stdNullPtr/wow-fishbot">Original C++ PoC</a>
  </p>
</div>

## ⚠️ Disclaimer

This tool is for educational purposes only. Using automation tools may violate the Terms of Service of World of Warcraft and could result in account suspension or ban. The authors are not responsible for any consequences of using this software.

## 📖 Table of Contents

- [Features](#features)
- [How It Works](#how-it-works)
- [Requirements](#requirements)
- [Getting Started](#getting-started)
  - [Installation](#installation)
  - [Building from Source](#building-from-source)
- [Configuration](#configuration)
- [Usage](#usage)
- [Troubleshooting](#troubleshooting)
- [Architecture](#architecture)
- [Contributing](#contributing)
- [License](#license)

## ✨ Features

- **Out-of-process operation** - Uses Windows API hooks without injecting into the game
- **Universal compatibility** - Works with any WoW version (Classic, TBC, WotLK, Retail)
- **Audio-based detection** - Detects fish by monitoring Windows master volume
- **Cursor change detection** - Identifies bobber location via cursor icon changes
- **Automatic lure application** - Supports up to two different lures with configurable timers
- **Wintergrasp support** - Automatically logs out during Wintergrasp battles (WotLK)
- **State machine architecture** - Predictable and debuggable behavior
- **Configuration validation** - Clear error messages for misconfigured settings
- **Resource efficient** - Minimal CPU usage with optimized polling

## 🔧 How It Works

Phishy uses a clever combination of Windows APIs to automate fishing without reading or modifying game memory:

1. **The Eyes** 👁️ - Monitors cursor icon changes (`EVENT_OBJECT_NAMECHANGE`) to detect when hovering over the fishing bobber
2. **The Ears** 👂 - Listens for volume spikes in Windows master audio to detect fish splashing
3. **The Brain** 🧠 - State machine that coordinates the fishing process
4. **The Hands** ✋ - Simulates mouse clicks and keyboard inputs to catch fish

### State Machine Flow

```
Start → Apply Lure (optional) → Cast Line → Find Bobber → Wait for Fish → Catch Fish → Repeat
                                                                    ↓
                                                          Logout (Wintergrasp) → Wait → Login
```

## 💻 Requirements

- **Operating System**: Windows 10/11 (Windows-specific APIs)
- **.NET Runtime**: .NET 7.0 or .NET 8.0 Desktop Runtime
- **Development** (if building from source):
  - Visual Studio 2022 with .NET Desktop workload
  - .NET 7.0/8.0 SDK

## 🚀 Getting Started

### Installation

1. Download the latest release from the [Releases](https://github.com/stdNullPtr/Phishy/releases) page
2. Extract the ZIP file to a folder of your choice
3. Run `guess.exe` (intentionally generic name)
4. On first run, a `configuration.yaml` file will be created and opened in Notepad
5. Configure the settings according to your WoW setup (see [Configuration](#configuration))
6. Run `guess.exe` again to start fishing

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/stdNullPtr/Phishy.git
   cd Phishy
   ```

2. Open `Phishy.sln` in Visual Studio 2022

3. Build the solution (Ctrl+Shift+B) in Release mode

4. Find the executable at: `Phishy\bin\Release\net7.0-windows\guess.exe`

Alternatively, using the command line:
```bash
dotnet build -c Release
```

## ⚙️ Configuration

The bot uses a YAML configuration file (`configuration.yaml`) with the following options:

```yaml
# Window Configuration
game-window-name: World of Warcraft  # Must match your WoW window title exactly

# Keybinds (use lowercase letters)
keyboard-key-start-fishing: 1        # Key bound to fishing ability
keyboard-key-apply-lure: 2          # Key bound to first lure (optional)
keyboard-key-apply-second-lure: 3   # Key bound to second lure (optional)
keyboard-key-logout: l              # Key bound to /logout macro (for Wintergrasp)

# Lure Settings
lure-buff-duration-minutes: 10      # Duration of first lure buff
second-lure-buff-duration-minutes: 5 # Duration of second lure buff (optional)

# Fishing Settings
fishing-channel-duration-seconds: 21 # How long to wait for a fish (default: 21)

# Audio Settings
setup-sound: true                   # Auto-configure Windows volume settings

# Wintergrasp Settings (WotLK only)
wait-for-wintergrasp: false         # Enable Wintergrasp logout/login cycle
```

### Configuration Tips

- **Window Name**: Must match exactly (case-sensitive). Common values:
  - `World of Warcraft` (Retail/Classic)
  - `World of Warcraft Classic`
  - Custom names if you've renamed your window
  
- **Keybinds**: Use single lowercase letters or numbers that match your in-game keybinds

- **Lure Duration**: Set slightly lower than actual buff duration to ensure reapplication

- **Channel Duration**: Default is 21 seconds, increase if you have fishing skill bonuses

## 📋 Usage

### Initial Setup

1. **In-game preparation**:
   - Bind your fishing ability to key `1` (or configure differently)
   - Bind your lure(s) to keys `2` and `3` (optional)
   - Create a `/logout` macro and bind it (for Wintergrasp feature)
   - Position yourself at a fishing spot
   - Zoom in completely (first-person view works best)
   - Set game sound to ~80%, disable ambient sounds

2. **Windows preparation**:
   - The bot will automatically set Windows volume to maximum and mute it
   - Ensure no other applications are making sounds

3. **Running the bot**:
   - Start `guess.exe`
   - Focus the WoW window
   - The bot will begin fishing automatically
   - Press `DELETE` key to stop

### Best Practices

- Test manual fishing first to ensure bobber lands near screen center
- Fish in quiet areas to avoid sound interference
- Keep the WoW window in focus and don't minimize it
- Don't move the mouse while the bot is running

## 🔍 Troubleshooting

### Common Issues

**"Failed retrieving window handle"**
- Ensure the window name in config matches exactly
- WoW must be running before starting the bot

**Bot doesn't detect bobber**
- Zoom in completely
- Ensure bobber lands near screen center
- Try adjusting camera angle
- Check that cursor changes to "interact" icon over bobber

**Bot doesn't catch fish**
- Increase game sound volume
- Disable all ambient sounds
- Ensure Windows volume is not muted by other apps
- Fish in quieter areas

**Configuration validation errors**
- Check the error message for specific issues
- Ensure all required fields are filled
- Verify keybinds are single characters

### Debug Mode

Run from Visual Studio in Debug mode to see detailed logging output.

## 🏗️ Architecture

The project follows SOLID principles with recent architectural improvements:

### Core Components

- **State Machine** (`FishingStateMachine.cs`) - Manages fishing states and transitions
- **Hooks** - Windows API hooks for input/output:
  - `WinEventHook` - Cursor change detection
  - `MouseHook` - Mouse input monitoring
  - `KeyboardHook` - Keyboard input monitoring
- **Services** - Business logic implementations:
  - `AudioDetector` - Sound detection
  - `WindowManager` - Window operations
  - `InputSimulator` - Input simulation
  - `ConsoleLogger` - Logging
- **Utils** - Low-level Windows API wrappers
- **Interfaces** - Contracts for dependency injection

### Recent Improvements

- Thread-safe operations with proper locking
- Comprehensive error handling with Win32 error codes
- Resource disposal for COM objects
- Configuration validation
- Reduced CPU usage by 80%
- Clean architecture with interfaces

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

Please ensure your code follows the existing patterns and includes appropriate error handling.

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Original C++ proof-of-concept: [wow-fishbot](https://github.com/stdNullPtr/wow-fishbot)
- Windows API documentation and community
- NAudio library for audio processing

---

<div align="center">
  Made with ❤️ by <a href="https://github.com/stdNullPtr">stdNullPtr</a>
</div>