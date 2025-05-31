namespace Phishy.Interfaces;

public interface IAudioDetector
{
    float GetMasterVolumeLevel();
    void SetVolumeToMax();
    void MuteSound();
}