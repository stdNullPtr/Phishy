namespace Phishy.Interfaces;

public interface IInputSimulator
{
    void SendMouseInput(MouseButtons button);
    void MoveToCenterOfWindow(string windowName, bool smoothly, int heightCorrectionPixels);
    void MoveMouseFibonacci(CancellationToken cancellationToken, string windowName, Func<bool> isBobberFound);
    void SendKeyInput(string key);
}