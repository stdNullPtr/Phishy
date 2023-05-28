namespace wow_fishbot_sharp.Utils;

internal class KeyboardUtils
{
    public static void SendKeyInput(Keys key)
    {
        string keyString = key.ToString();
        if (key is >= Keys.D0 and <= Keys.D9)
        {
            // For digits ('0' to '9'), convert the character to a string
            keyString = keyString[1..];
        }

        // Simulate pressing a key
        SendKeys.SendWait("{" + keyString + "}");
    }
}