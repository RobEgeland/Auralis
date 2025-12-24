using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Audio.Wasapi
{
    internal sealed class WasapiSessionLocator : IDisposable
    {
        private readonly MMDeviceEnumerator _enumerator = new();
        private MMDevice? _device;
        private AudioSessionManager? _manager;
        private readonly Dictionary<int, List<AudioSessionControl>> _sessions = new();
        private bool _disposed;

        public WasapiSessionLocator()
        {
            Initialize();
        }

        private void Initialize()
        {
            _device = _enumerator.GetDefaultAudioEndpoint(
                DataFlow.Render,
                Role.Multimedia);

            _manager = _device.AudioSessionManager;

            var collection = _manager.Sessions;

            for (int i = 0; i < collection.Count; i++)
            {
                AudioSessionControl session;
                try
                {
                    session = collection[i];
                }
                catch
                {
                    continue;
                }

                int pid;
                try
                {
                    pid = (int)session.GetProcessID;
                }
                catch
                {
                    continue;
                }

                if (!_sessions.TryGetValue(pid, out var list))
                {
                    list = new List<AudioSessionControl>();
                    _sessions[pid] = list;
                }

                list.Add(session);
            }
        }

        public IReadOnlyList<AudioSessionControl> GetSessionsForProcess(int pid)
        {
            if (_disposed)
                return Array.Empty<AudioSessionControl>();

            return _sessions.TryGetValue(pid, out var list)
                ? list
                : Array.Empty<AudioSessionControl>();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            foreach (var list in _sessions.Values)
                foreach (var s in list)
                    s.Dispose();

            _sessions.Clear();
            _manager?.Dispose();
            _device?.Dispose();
            _enumerator.Dispose();
        }
    }

}
