using System.Text;

namespace Phishy.Configs;

public class ConfigValidator
{
    public static ValidationResult Validate(Properties properties)
    {
        var errors = new List<string>();

        // Window name validation
        if (string.IsNullOrWhiteSpace(properties.GameWindowName))
        {
            errors.Add("GameWindowName is required");
        }

        // Keyboard key validations
        if (string.IsNullOrWhiteSpace(properties.KeyboardKeyStartFishing))
        {
            errors.Add("KeyboardKeyStartFishing is required");
        }

        // Timing validations
        if (properties.FishingChannelDurationSeconds < 1 || properties.FishingChannelDurationSeconds > 60)
        {
            errors.Add("FishingChannelDurationSeconds must be between 1 and 60 seconds");
        }

        if (properties.LureBuffDurationMinutes < 0 || properties.LureBuffDurationMinutes > 60)
        {
            errors.Add("LureBuffDurationMinutes must be between 0 and 60 minutes");
        }

        if (properties.SecondLureBuffDurationMinutes.HasValue)
        {
            if (properties.SecondLureBuffDurationMinutes.Value < 0 || properties.SecondLureBuffDurationMinutes.Value > 60)
            {
                errors.Add("SecondLureBuffDurationMinutes must be between 0 and 60 minutes");
            }

            if (string.IsNullOrWhiteSpace(properties.KeyboardKeyApplySecondLure))
            {
                errors.Add("KeyboardKeyApplySecondLure is required when SecondLureBuffDurationMinutes is set");
            }
        }

        // Logout/login key validations
        if (properties.WaitForWintergrasp)
        {
            if (string.IsNullOrWhiteSpace(properties.KeyboardPressLogout))
            {
                errors.Add("KeyboardPressLogout is required when WaitForWintergrasp is enabled");
            }
        }

        return new ValidationResult(errors);
    }
}

public class ValidationResult
{
    public List<string> Errors { get; }
    public bool IsValid => Errors.Count == 0;

    public ValidationResult(List<string> errors)
    {
        Errors = errors ?? new List<string>();
    }

    public override string ToString()
    {
        if (IsValid)
        {
            return "Configuration is valid";
        }

        var sb = new StringBuilder();
        sb.AppendLine("Configuration validation failed:");
        foreach (var error in Errors)
        {
            sb.AppendLine($"  - {error}");
        }
        return sb.ToString();
    }
}