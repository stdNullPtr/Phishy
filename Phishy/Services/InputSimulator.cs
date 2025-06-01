using Phishy.Interfaces;
using Phishy.Utils;

namespace Phishy.Services;

public class InputSimulator : IInputSimulator
{
    public void SendMouseInput(MouseButtons button)
    {
        MouseUtils.SendMouseInput(button);
    }

    public void MoveToCenterOfWindow(string windowName, bool smoothly, int heightCorrectionPixels)
    {
        MouseUtils.MoveToCenterOfWindow(windowName, smoothly, heightCorrectionPixels);
    }

    public void MoveMouseFibonacci(CancellationToken cancellationToken, string windowName, Func<bool> isBobberFound)
    {
        MouseUtils.MoveMouseFibonacci(cancellationToken, windowName, isBobberFound);
    }

    public void SendKeyInput(string key)
    {
        KeyboardUtils.SendKeyInput(key);
    }
}