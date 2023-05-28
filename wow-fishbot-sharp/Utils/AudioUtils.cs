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
}