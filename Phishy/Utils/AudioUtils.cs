using NAudio.CoreAudioApi;


namespace Phishy.Utils;

internal class AudioUtils
{
    public static float GetMasterVolumeLevel()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            float peakValue = device.AudioMeterInformation.MasterPeakValue;
            // Log every 10th call to avoid spam, or when value is significant
            if (Random.Shared.Next(10) == 0 || peakValue > 0.01f)
            {
                Console.WriteLine($"[AudioUtils]: Master peak value: {peakValue:F4}");
            }
            return peakValue;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AudioUtils]: Error getting master volume level: {ex.Message}");
            return 0f;
        }
    }

    public static void SetVolumeToMax()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            
            float currentVolume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            Console.WriteLine($"[AudioUtils]: Current volume before setting: {currentVolume:F2}");
            
            device.AudioEndpointVolume.MasterVolumeLevelScalar = 1.0f;
            
            float newVolume = device.AudioEndpointVolume.MasterVolumeLevelScalar;
            Console.WriteLine($"[AudioUtils]: Volume after setting to max: {newVolume:F2}");
            
            if (Math.Abs(newVolume - 1.0f) > 0.01f)
            {
                Console.WriteLine($"[AudioUtils]: WARNING - Failed to set volume to max! Current: {newVolume:F2}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AudioUtils]: Error setting volume to max: {ex.Message}");
        }
    }

    public static void MuteSound()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            
            bool wasMuted = device.AudioEndpointVolume.Mute;
            Console.WriteLine($"[AudioUtils]: Mute status before: {wasMuted}");
            
            device.AudioEndpointVolume.Mute = true;
            
            bool isMuted = device.AudioEndpointVolume.Mute;
            Console.WriteLine($"[AudioUtils]: Mute status after: {isMuted}");
            
            if (!isMuted)
            {
                Console.WriteLine("[AudioUtils]: WARNING - Failed to mute audio!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AudioUtils]: Error muting sound: {ex.Message}");
        }
    }

    public static void LogAudioDeviceInfo()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            
            Console.WriteLine("[AudioUtils]: === Audio Device Diagnostic Info ===");
            Console.WriteLine($"[AudioUtils]: Device Name: {device.FriendlyName}");
            Console.WriteLine($"[AudioUtils]: Device State: {device.State}");
            Console.WriteLine($"[AudioUtils]: Device ID: {device.ID}");
            
            var volume = device.AudioEndpointVolume;
            Console.WriteLine($"[AudioUtils]: Volume Range: {volume.VolumeRange.MinDecibels}dB to {volume.VolumeRange.MaxDecibels}dB");
            Console.WriteLine($"[AudioUtils]: Current Volume (Scalar): {volume.MasterVolumeLevelScalar:F2}");
            Console.WriteLine($"[AudioUtils]: Current Volume (dB): {volume.MasterVolumeLevel:F2}dB");
            Console.WriteLine($"[AudioUtils]: Is Muted: {volume.Mute}");
            
            // Check if audio meter is working
            var meter = device.AudioMeterInformation;
            Console.WriteLine($"[AudioUtils]: Current Peak Value: {meter.MasterPeakValue:F4}");
            
            // Check each channel
            int channelCount = meter.PeakValues.Count;
            Console.WriteLine($"[AudioUtils]: Number of channels: {channelCount}");
            for (int i = 0; i < channelCount; i++)
            {
                Console.WriteLine($"[AudioUtils]: Channel {i} peak: {meter.PeakValues[i]:F4}");
            }
            Console.WriteLine("[AudioUtils]: ===================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AudioUtils]: Error getting audio device info: {ex.Message}");
            Console.WriteLine($"[AudioUtils]: Stack trace: {ex.StackTrace}");
        }
    }

    public static void TestAudioDetection(int durationSeconds = 10)
    {
        Console.WriteLine($"[AudioUtils]: Starting audio detection test for {durationSeconds} seconds...");
        Console.WriteLine("[AudioUtils]: Make some noise to test detection!");
        
        try
        {
            LogAudioDeviceInfo();
            
            DateTime startTime = DateTime.Now;
            float maxDetected = 0f;
            int detectionCount = 0;
            
            while ((DateTime.Now - startTime).TotalSeconds < durationSeconds)
            {
                float level = GetMasterVolumeLevel();
                if (level > 0.001f)
                {
                    detectionCount++;
                    if (level > maxDetected)
                    {
                        maxDetected = level;
                    }
                    Console.WriteLine($"[AudioUtils]: Sound detected! Level: {level:F4}");
                }
                
                Thread.Sleep(50); // Check 20 times per second
            }
            
            Console.WriteLine($"[AudioUtils]: Test complete. Max level: {maxDetected:F4}, Detections: {detectionCount}");
            
            if (maxDetected == 0f)
            {
                Console.WriteLine("[AudioUtils]: WARNING - No audio was detected during the test!");
                Console.WriteLine("[AudioUtils]: Possible causes:");
                Console.WriteLine("[AudioUtils]: 1. Audio service not accessible from WSL");
                Console.WriteLine("[AudioUtils]: 2. No audio playing or microphone input");
                Console.WriteLine("[AudioUtils]: 3. Audio device is muted at hardware level");
                Console.WriteLine("[AudioUtils]: 4. COM interop issues in WSL environment");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AudioUtils]: Test failed with error: {ex.Message}");
        }
    }
}