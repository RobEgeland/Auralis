using Auralis.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Engine
{
    public sealed class AudioSourceRegistry : IAudioSourceRegistry
    {
        private readonly Dictionary<string, HashSet<int>> _pidMap = new();
        private readonly object _lock = new();

        public void ResolveAndRegister(AudioSource source)
        {
            if (string.IsNullOrWhiteSpace(source.SourceKey))
                return;

            var processName =
                Path.GetFileNameWithoutExtension(source.SourceKey);

            var pids = Process
                .GetProcessesByName(processName)
                .Select(p => p.Id)
                .ToHashSet();

            lock (_lock)
            {
                _pidMap[source.SourceKey] = pids;
            }
        }

        public IReadOnlyCollection<int> GetPids(string sourceKey)
        {
            lock (_lock)
            {
                return _pidMap.TryGetValue(sourceKey, out var pids)
                    ? pids
                    : Array.Empty<int>();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pidMap.Clear();
            }
        }

        public bool TryGetPid(string sourceKey, out int pid)
        {
            pid = default;

            if (string.IsNullOrWhiteSpace(sourceKey))
                return false;

            lock (_lock)
            {
                if (!_pidMap.TryGetValue(sourceKey, out var pids) || pids.Count == 0)
                    return false;

                // Prefer a PID that is still alive
                foreach (var candidate in pids)
                {
                    try
                    {
                        var process = Process.GetProcessById(candidate);
                        if (!process.HasExited)
                        {
                            pid = candidate;
                            return true;
                        }
                    }
                    catch
                    {
                        // PID is stale; ignore
                    }
                }

                return false;
            }
        }

    }


}
