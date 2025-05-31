using NAudio.CoreAudioApi;


namespace Phishy.Utils;

internal class AudioUtils
{
    public static float GetMasterVolumeLevel()
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        return device.AudioMeterInformation.MasterPeakValue;
    }

    public static void SetVolumeToMax()
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.MasterVolumeLevelScalar = 1.0f;
    }

    public static void MuteSound()
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.Mute = true;
    }
}