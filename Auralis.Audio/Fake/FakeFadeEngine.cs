using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auralis.Core.Engine;

namespace Auralis.Audio.Fake
{
    public sealed class FakeFadeEngine : IFadeEngine
    {
        public bool IsBusy { get; private set; }
        public async Task FadeToAsync(
            string key,
            float targetVolumeDb,
            int durationMs,
            CancellationToken ct)
        {
            // Simulate fade duration
            await Task.Delay(durationMs, ct);
        }
    }
}
