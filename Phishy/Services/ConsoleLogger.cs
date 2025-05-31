using Phishy.Interfaces;

namespace Phishy.Services;

public class ConsoleLogger : ILogger
{
    private readonly string _prefix;
    private readonly object _lock = new object();

    public ConsoleLogger(string prefix = "")
    {
        _prefix = string.IsNullOrEmpty(prefix) ? "" : $"[{prefix}]: ";
    }

    public void LogInfo(string message)
    {
        Log("INFO", message, ConsoleColor.White);
    }

    public void LogWarning(string message)
    {
        Log("WARN", message, ConsoleColor.Yellow);
    }

    public void LogError(string message, Exception? exception = null)
    {
        var fullMessage = exception != null 
            ? $"{message} - {exception.GetType().Name}: {exception.Message}" 
            : message;
        
        Log("ERROR", fullMessage, ConsoleColor.Red);
        
        if (exception?.StackTrace != null)
        {
            Log("ERROR", exception.StackTrace, ConsoleColor.DarkRed);
        }
    }

    public void LogDebug(string message)
    {
        #if DEBUG
        Log("DEBUG", message, ConsoleColor.Gray);
        #endif
    }

    private void Log(string level, string message, ConsoleColor color)
    {
        lock (_lock)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var originalColor = Console.ForegroundColor;
            
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{timestamp}] ");
            
            Console.ForegroundColor = color;
            Console.Write($"[{level}] ");
            
            Console.ForegroundColor = originalColor;
            Console.WriteLine($"{_prefix}{message}");
        }
    }
}