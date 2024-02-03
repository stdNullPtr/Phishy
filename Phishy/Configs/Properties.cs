using Phishy.Utils;
using YamlDotNet.Serialization;

namespace Phishy.Configs;

public sealed class Properties
{
    [YamlMember(Description = "Keyboard key binding for logout macro (Example: 3)")]
    public string KeyboardPressLogout { get; set; }
    [YamlMember(Description = "Whether or not to enable logout during Wintergrasp (Example: false)")]
    public bool WaitForWintergrasp { get; set; }
    [YamlMember(Description = "Keyboard key binding for fishing cast (Example: 1)")]
    public string KeyboardKeyStartFishing { get; set; }
    [YamlMember(Description = "Keyboard key binding for applying lure (optional)")]
    public string KeyboardKeyApplyLure { get; set; }
    [YamlMember(Description = "Keyboard key binding for applying a second lure (optional)")]
    public string? KeyboardKeyApplySecondLure { get; set; }
    [YamlMember(Description = "Buff duration of first lure (mandatory if first lure keybind is set)")]
    public int LureBuffDurationMinutes { get; set; }
    [YamlMember(Description = "Buff duration of second lure (mandatory if second lure keybind is set)")]
    public int? SecondLureBuffDurationMinutes { get; set; }
    [YamlMember(Description = "Fishing cast duration in seconds")]
    public int FishingChannelDurationSeconds { get; set; }
    [YamlMember(Description = "Window name of the game, when you hover over it in the taskbar")]
    public string GameWindowName { get; set; }

    public Properties()
    {
        KeyboardPressLogout = KeyboardUtils.ConvertToString(Keys.D3);
        WaitForWintergrasp = false;
        KeyboardKeyStartFishing = KeyboardUtils.ConvertToString(Keys.D1);
        KeyboardKeyApplyLure = KeyboardUtils.ConvertToString(Keys.D2);
        KeyboardKeyApplySecondLure = null;
        LureBuffDurationMinutes = TimeSpan.FromMinutes(10).Minutes;
        SecondLureBuffDurationMinutes = null;
        FishingChannelDurationSeconds = TimeSpan.FromSeconds(20).Seconds;
        GameWindowName = "Game Window Name";
    }
}