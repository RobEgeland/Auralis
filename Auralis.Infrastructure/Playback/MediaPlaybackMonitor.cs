using Auralis.Core.Playback;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace Auralis.Infrastructure.Playback
{
    public sealed class MediaPlaybackMonitor : IMediaPlaybackMonitor, IDisposable
    {
        private GlobalSystemMediaTransportControlsSessionManager? _manager;

        // Tracks playback state by source key (e.g. "chrome.exe")
        private readonly ConcurrentDictionary<string, bool> _playingBySource =
            new(StringComparer.OrdinalIgnoreCase);

        private bool _initialized;

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            _manager =
                await GlobalSystemMediaTransportControlsSessionManager
                    .RequestAsync();

            _manager.SessionsChanged += OnSessionsChanged;

            // Prime state
            UpdateAllSessions();

            _initialized = true;
        }

        /// <summary>
        /// Returns true if SMTC reports the source as currently playing.
        /// </summary>
       
       public bool IsPrimaryPlaying(string sourceKey)
        {
            var key = NormalizeSourceKey(sourceKey);
            //Debug.WriteLine(
            //$"SMTC keys present: {string.Join(", ", _playingBySource.Keys)} | queried: {sourceKey}");

            return _playingBySource.TryGetValue(key, out var playing)
                   && playing;
        }

        private void OnSessionsChanged(
            GlobalSystemMediaTransportControlsSessionManager sender,
            SessionsChangedEventArgs args)
        {
            UpdateAllSessions();
        }

        private void UpdateAllSessions()
        {
            if (_manager == null)
                return;

            _playingBySource.Clear();

            foreach (var session in _manager.GetSessions())
            {
                try
                {
                    session.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                    session.PlaybackInfoChanged += OnPlaybackInfoChanged;

                    UpdateSessionState(session);
                }
                catch
                {
                    // Ignore transient SMTC failures
                }
            }
        }

        private void OnPlaybackInfoChanged(
    GlobalSystemMediaTransportControlsSession sender,
    PlaybackInfoChangedEventArgs args)
        {
            UpdateSessionState(sender);
        }

        private void UpdateSessionState(
    GlobalSystemMediaTransportControlsSession session)
        {
            var rawKey = ExtractSourceKey(session);
            if (rawKey == null)
                return;

            var key = NormalizeSourceKey(rawKey);

            var playbackInfo = session.GetPlaybackInfo();
            bool isPlaying =
                playbackInfo.PlaybackStatus ==
                GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

            _playingBySource[key] = isPlaying;
        }

        /// <summary>
        /// Best-effort mapping from SMTC session to your AudioSource.SourceKey.
        /// </summary>
        private static string? ExtractSourceKey(
            GlobalSystemMediaTransportControlsSession session)
        {
            var appId = session.SourceAppUserModelId;
            if (string.IsNullOrWhiteSpace(appId))
                return null;

            appId = appId.ToLowerInvariant();

            // Chrome / Edge / browsers
            if (appId.Contains("chrome"))
                return "chrome.exe";

            if (appId.Contains("edge"))
                return "msedge.exe";

            if (appId.Contains("spotify"))
                return "spotify.exe";

            // Fallback: try to extract exe-like token
            var token = appId.Split('!', '.', '_').FirstOrDefault();
            if (string.IsNullOrWhiteSpace(token))
                return null;

            return token.EndsWith(".exe")
                ? token
                : token + ".exe";
        }

        private static string NormalizeSourceKey(string sourceKey)
        {
            if (string.IsNullOrWhiteSpace(sourceKey))
                return string.Empty;

            sourceKey = sourceKey.ToLowerInvariant();

            // Strip .exe if present
            if (sourceKey.EndsWith(".exe"))
                sourceKey = sourceKey[..^4];

            // Canonical browser mappings
            if (sourceKey.Contains("chrome"))
                return "chrome";

            if (sourceKey.Contains("edge"))
                return "edge";

            if (sourceKey.Contains("spotify"))
                return "spotify";

            return sourceKey;
        }



        public void Dispose()
        {
            if (_manager != null)
            {
                _manager.SessionsChanged -= OnSessionsChanged;
                _manager = null;
            }

            _playingBySource.Clear();
        }
    }
}
