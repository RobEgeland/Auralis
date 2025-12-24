using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auralis.Core.Engine;

namespace Auralis.Audio.Fake
{
    public sealed class FakeAudioMeter : IAudioMeter
    {
        public float GetCurrentLevelDb(string key)
        {
            // Mostly silence, occasional "activity"
            return Random.Shared.NextDouble() > 0.95
            ? -18f
            : -80f; ;
        }
    }
}
