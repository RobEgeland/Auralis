using System;
using System.Diagnostics;
using Auralis.Core.Engine;
using NAudio.CoreAudioApi;

public sealed class WasapiAudioMeter : IAudioMeter, IDisposable
{
    private readonly WasapiThread _thread;
    private MMDevice _device = null!;

    public WasapiAudioMeter()
    {
        _thread = new WasapiThread();

        _thread.Invoke(() =>
        {
            var enumerator = new MMDeviceEnumerator();
            _device = enumerator.GetDefaultAudioEndpoint(
                DataFlow.Render,
                Role.Multimedia);
            return 0;
        });
    }

    /// <summary>
    /// Returns peak level (dBFS) for all sessions belonging to the given process name.
    /// Example sourceKey: "chrome.exe", "Spotify.exe"
    /// </summary>
    public float GetCurrentLevelDb(string sourceKey)
    {
        return _thread.Invoke(() =>
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(
                DataFlow.Render,
                Role.Multimedia);

            var sessions = device.AudioSessionManager.Sessions;
            float maxPeak = 0f;

            for (int i = 0; i < sessions.Count; i++)
            {
                try
                {
                    var s = sessions[i];
                    int pid = (int)s.GetProcessID;
                    var name = TryGetProcessName(pid);

                    if (!string.Equals(
                        name,
                        sourceKey,
                        StringComparison.OrdinalIgnoreCase))
                        continue;

                    maxPeak = Math.Max(
                        maxPeak,
                        s.AudioMeterInformation.MasterPeakValue);
                }
                catch
                {
                    // dead session — ignore
                }
            }

            return maxPeak > 0
                ? 20f * (float)Math.Log10(maxPeak)
                : -100f;
        });
    }

    private static bool IsMatch(string processName, string sourceKey)
    {
        if (processName == null)
            return false;

        return processName.Equals(sourceKey, StringComparison.OrdinalIgnoreCase);
    }

    private static string TryGetProcessName(int pid)
    {
        try
        {
            return Process.GetProcessById(pid).ProcessName + ".exe";
        }
        catch
        {
            return null;
        }
    }

    private static float LinearToDb(float value)
    {
        return 20f * (float)Math.Log10(value);
    }
    public void Dispose()
    {
        _thread.Dispose();
    }
}
