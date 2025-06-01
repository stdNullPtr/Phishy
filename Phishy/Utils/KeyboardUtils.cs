namespace Phishy.Utils;

internal class KeyboardUtils
{
    public static void SendKeyInput(Keys key)
    {
        string keyString = ConvertToString(key);
        SendKeyInput(keyString);
    }

    public static void SendKeyInput(string key)
    {
        // For single letter keys, send without braces to get lowercase
        // For special keys (like numbers converted from Keys.D1), use braces
        if (key.Length == 1 && char.IsLetter(key[0]))
        {
            SendKeys.SendWait(key.ToLower());
        }
        else
        {
            SendKeys.SendWait("{" + key + "}");
        }
    }

    public static string ConvertToString(Keys key)
    {
        string keyString = key.ToString();
        if (key is >= Keys.D0 and <= Keys.D9)
        {
            // For digits ('0' to '9'), convert the character to a string
            keyString = keyString[1..];
        }
        return keyString;
    }
}