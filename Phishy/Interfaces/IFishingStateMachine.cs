namespace Phishy.Interfaces;

public interface IFishingStateMachine
{
    void Update(CancellationToken cancellationToken);
    void NotifyBobberFound();
}