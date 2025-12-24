using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using Auralis.Core.Models;

namespace Auralis.Audio.Wasapi
{
    public sealed class WasapiAudioSourceEnumerator : IDisposable
    {
        private readonly MMDeviceEnumerator _enumerator = new();
        private MMDevice _device;

        public WasapiAudioSourceEnumerator()
        {
            _device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }

        public IReadOnlyList<AudioSource> GetActiveSources()
        {
            var sessions = _device.AudioSessionManager.Sessions;
            var map = new Dictionary<string, AudioSource>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];

                uint pid;
                try { pid = session.GetProcessID; }
                catch { continue; }

                string processName;
                try
                {
                    processName = System.Diagnostics.Process
                        .GetProcessById((int)pid)
                        .ProcessName + ".exe";
                }
                catch
                {
                    continue;
                }

                if (!map.ContainsKey(processName))
                {
                    map[processName] = new AudioSource
                    {
                        SourceKey = processName,
                        DisplayName = ToFriendlyName(processName)
                    };
                }
            }

            return map.Values
                .OrderBy(s => s.DisplayName)
                .ToList();
        }

        private static string ToFriendlyName(string exe)
        => exe.Replace(".exe", "", StringComparison.OrdinalIgnoreCase);

        public void Dispose()
        {
            _device.Dispose();
            _enumerator.Dispose();
        }
    }
}
