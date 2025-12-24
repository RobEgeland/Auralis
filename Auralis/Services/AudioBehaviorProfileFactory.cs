using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auralis.Core.Models;

namespace Auralis.Services
{
    public static class AudioBehaviorProfileFactory
    {
        public static AudioBehaviorProfile CreateDefaultProfile()
        {
            return new AudioBehaviorProfile
            {
                Primary = new AudioSource
                {
                    SourceKey = string.Empty,
                    DisplayName = "Not configured"
                },
                Secondary = new AudioSource
                {
                    SourceKey = string.Empty,
                    DisplayName = "Not configured"
                },

                SilenceThresholdDb = -45f,
                SilenceHoldMs = 1200,
                FadeInMs = 350,
                FadeOutMs = 450,
                SecondaryMaxVolume = 0.5f,
                SecondaryMinVolume = 0.0f
            };
        }

    }
}
