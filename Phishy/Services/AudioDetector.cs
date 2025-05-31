using Phishy.Interfaces;
using Phishy.Utils;

namespace Phishy.Services;

public class AudioDetector : IAudioDetector
{
    public float GetMasterVolumeLevel()
    {
        return AudioUtils.GetMasterVolumeLevel();
    }

    public void SetVolumeToMax()
    {
        AudioUtils.SetVolumeToMax();
    }

    public void MuteSound()
    {
        AudioUtils.MuteSound();
    }
}