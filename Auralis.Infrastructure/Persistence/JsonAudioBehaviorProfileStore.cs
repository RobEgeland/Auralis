using Auralis.Core.Models;
using Auralis.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Auralis.Infrastructure.Persistence
{
    public sealed class JsonAudioBehaviorProfileStore : IAudioBehaviorProfileStore
    {
        private readonly string _filePath;
        public JsonAudioBehaviorProfileStore()
        {
            var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Auralis");

            Directory.CreateDirectory(folder);

            _filePath = Path.Combine(folder, "profile.json");
        }

        public AudioBehaviorProfile? LoadProfile()
        {
            if (!File.Exists(_filePath))
                return null;

            var json = File.ReadAllText(_filePath);
            var profile = JsonSerializer.Deserialize<AudioBehaviorProfile>(json);

            if (profile == null)
                return null;

            return new AudioBehaviorProfile
            {
                Primary = profile.Primary,
                Secondary = profile.Secondary,

                SilenceThresholdDb = profile.SilenceThresholdDb,
                SilenceHoldMs = profile.SilenceHoldMs,
                FadeInMs = profile.FadeInMs,
                FadeOutMs = profile.FadeOutMs,

                SecondaryMaxVolume = NormalizeVolume(profile.SecondaryMaxVolume),
                SecondaryMinVolume = NormalizeVolume(profile.SecondaryMinVolume)
            };
        }

        public void SaveProfile(AudioBehaviorProfile profile)
        {
            var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_filePath, json);
        }

        private static float NormalizeVolume(float v)
        {
            // Migration: old profiles may store 0–100
            if (v > 1f)
                return Math.Clamp(v / 100f, 0f, 1f);

            return Math.Clamp(v, 0f, 1f);
        }
    }
}
