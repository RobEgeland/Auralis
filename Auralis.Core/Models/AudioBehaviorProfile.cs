using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auralis.Core.Models
{
    public sealed class AudioBehaviorProfile
    {
        public AudioSource Primary { get; init; } = null!;
        public AudioSource Secondary { get; init; } = null!;

        public float SilenceThresholdDb { get; init; } = -45f;
        public int SilenceHoldMs { get; init; } = 1200;

        public int FadeInMs { get; init; } = 350;
        public int FadeOutMs { get; init; } = 450;

        // Volume range: 0.0f – 1.0f
        public float SecondaryMaxVolume { get; init; } = 0.5f;
        public float SecondaryMinVolume { get; init; } = 0.0f;
    }
}
