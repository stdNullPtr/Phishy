using NAudio.CoreAudioApi;


namespace wow_fishbot_sharp.Utils;

internal class AudioUtils
{
    public static float GetMasterVolumeLevel()
    {
        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        AudioMeterInformation meter = device.AudioMeterInformation;
        float volumeLevel = meter.MasterPeakValue;

        return volumeLevel;
    }

    public static void SetVolumeToMax()
    {
        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.MasterVolumeLevel = 0.0f;
    }

    public static void MuteSound()
    {
        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        MMDevice device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        device.AudioEndpointVolume.Mute = true;
    }
}