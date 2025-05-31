using System.Diagnostics;
using Phishy.Utils;

namespace Phishy.Configs;

public static class AppConfig
{
    public const string CONFIGURATION_FILE_NAME = "configuration.yaml";

    public static Properties Props { get; private set; } = new();

    public static bool LoadProperties()
    {
        if (!FileUtils.FileExistsInCurrentDirectory(CONFIGURATION_FILE_NAME))
        {
            Console.WriteLine("[AppConfig]: Configuration file is missing, a default one will be created and the application will exit!");
            YamlUtils.GenerateSampleRunConfig(CONFIGURATION_FILE_NAME);
            Process.Start("notepad.exe", CONFIGURATION_FILE_NAME);
            return false;
        }

        Console.WriteLine($"[AppConfig]: Loading configuration from {CONFIGURATION_FILE_NAME}...");
        Properties? props = YamlUtils.ReadPropertiesFromCurrentDirectory(CONFIGURATION_FILE_NAME);
        if (props is null)
        {
            Console.WriteLine($"[AppConfig]: Failed loading configuration from: '{CONFIGURATION_FILE_NAME}'!");
            return false;
        }

        Props = props;

        // Validate configuration
        var validationResult = ConfigValidator.Validate(Props);
        if (!validationResult.IsValid)
        {
            Console.WriteLine(validationResult.ToString());
            return false;
        }

        Console.WriteLine("[AppConfig]: Configuration validated successfully.");
        return true;
    }
}