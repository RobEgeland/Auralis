using Auralis.Core.Engine;
using Auralis.Core.Logging;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Audio.Wasapi
{
    public sealed class WasapiFadeEngine : IFadeEngine, IDisposable
    {
        public bool IsBusy { get; private set; }
        private readonly WasapiSessionLocator _locator = new();
        private IAuralisLogger? _logger;
        private readonly IAudioSourceRegistry _registry;
        public WasapiFadeEngine(IAuralisLogger? logger = null, AudioSourceRegistry registry = null)
        {
            _logger = logger;
            _registry = registry;
        }
        public async Task FadeToAsync(
        string sourceKey,
        float targetVolume,
        int durationMs,
        CancellationToken ct)
        {
            _logger?.Info(
                $"Fade request → PID={sourceKey}, target={targetVolume}, duration={durationMs}ms");

            // Clamp volume to 0..1
            IsBusy = true;
            try
            {
                targetVolume = Math.Clamp(targetVolume, 0f, 1f);


                var sessions = GetSessionsForSource(sourceKey);
                if (sessions.Count == 0)
                    return;

                // We fade all sessions for that process (some apps create multiple sessions)
                var startVolumes = sessions
                    .Select(s => SafeGetVolume(s))
                    .ToArray();

                const int stepMs = 25;
                int steps = Math.Max(1, durationMs / stepMs);

                for (int i = 1; i <= steps; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    float t = (float)i / steps;
                    for (int s = 0; s < sessions.Count; s++)
                    {
                        float v = Lerp(startVolumes[s], targetVolume, t);
                        SafeSetVolume(sessions[s], v);
                    }

                    await Task.Delay(stepMs, ct);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private List<AudioSessionControl> GetSessionsForSource(string sourceKey)
        {
            var pids = _registry.GetPids(sourceKey);
            if (pids.Count == 0)
                return new List<AudioSessionControl>();

            var sessions = new List<AudioSessionControl>();

            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(
                DataFlow.Render,
                Role.Multimedia);

            var sessionManager = device.AudioSessionManager;
            var collection = sessionManager.Sessions;

            for (int i = 0; i < collection.Count; i++)
            {
                var session = collection[i];
                try
                {
                    int pid = (int)session.GetProcessID; // This is allowed on AudioSessionControl
                    if (pids.Contains(pid))
                    {
                        sessions.Add(session);
                    }
                    else
                    {
                        session.Dispose();
                    }
                }
                catch
                {
                    session.Dispose();
                }
            }

            return sessions;
        }


        private static float SafeGetVolume(AudioSessionControl session)
        {
            try
            {
                return session.SimpleAudioVolume.Volume; // NAudio wraps SimpleAudioVolume :contentReference[oaicite:8]{index=8}
            }
            catch
            {
                return 1f;
            }
        }

        private static void SafeSetVolume(AudioSessionControl session, float volume)
        {
            try
            {
                session.SimpleAudioVolume.Volume = Math.Clamp(volume, 0f, 1f);
            }
            catch
            {
                // ignore sessions that vanish mid-fade
            }
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;
        public void Dispose() => _locator.Dispose();
    }
}
