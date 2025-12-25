using Auralis.Core.Logging;
using Auralis.Core.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Auralis.Core.Playback;

namespace Auralis.Core.Engine
{
    public enum AudioBehaviorState
    {
        Disabled,
        Monitoring,
        SecondaryActive
    }
    public sealed class AudioBehaviorCoordinator
    {
        public event Action<AudioBehaviorState>? StateChanged;
        private readonly IAudioMeter _meter;
        private readonly IFadeEngine _fader;
        private AudioBehaviorProfile _profile;
        private readonly SilenceDetector _silence;
        private IAuralisLogger? _logger;
        private readonly IMediaPlaybackMonitor? _playbackMonitor;

        public AudioBehaviorProfile CurrentProfile => _profile;
        private AudioSourceRegistry _registry;



        private AudioBehaviorState _state = AudioBehaviorState.Disabled;
        private CancellationTokenSource? _loopCts;
        private CancellationTokenSource? _fadeCts;

        private const int TickMs = 50;

        public AudioBehaviorCoordinator(
        IAudioMeter meter,
        IFadeEngine fader,
        AudioBehaviorProfile profile,
        AudioSourceRegistry registry,
        IMediaPlaybackMonitor? playbackMonitor,
        IAuralisLogger logger)
        {
            _meter = meter;
            _fader = fader;
            _profile = profile;
            _registry = registry;
            _playbackMonitor = playbackMonitor;
            _logger = logger;
            _silence = new SilenceDetector(
                profile.SilenceThresholdDb,
                profile.SilenceHoldMs);
        }

        public void Enable()
        {
            if (_state != AudioBehaviorState.Disabled)
                return;
            _logger?.Info("Coordinator ENABLED");
            SetState(AudioBehaviorState.Monitoring);
            _loopCts = new CancellationTokenSource();
            _ = Task.Run(() => LoopAsync(_loopCts.Token));
        }

        public void Disable()
        {
            _logger?.Info("Coordinator DISABLED");

            _loopCts?.Cancel();
            _fadeCts?.Cancel();
            SetState(AudioBehaviorState.Disabled);
        }

        private async Task LoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    Tick(TickMs);
                    await Task.Delay(TickMs, ct);
                }
            }
            catch (TaskCanceledException)
            {
                // Expected during shutdown
            }

        }

        //private async Task LoopAsync(CancellationToken ct)
        //{
        //    try
        //    {
        //        _logger.Info("LoopAsync started");

        //        while (!ct.IsCancellationRequested)
        //        {
        //            _logger.Info("Loop iteration start");

        //            try
        //            {
        //                Tick(TickMs);
        //                _logger.Info("Loop iteration end");
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.Error("Unhandled exception inside Tick()", ex);
        //                _logger.Info("Loop iteration recovered after exception");
        //            }

        //            await Task.Delay(TickMs, ct);
        //        }
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        _logger.Info("LoopAsync canceled");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error("Unhandled exception escaping LoopAsync()", ex);
        //    }
        //    finally
        //    {
        //        _logger.Info("LoopAsync exited");
        //    }
        //}


        private void Tick(int deltaMs)
        {
            _logger?.Info("Tick");

            float primaryDb = _meter.GetCurrentLevelDb(_profile.Primary.SourceKey);
            bool silentHeld = _silence.Update(primaryDb, deltaMs);
            bool mediaPlaying = _playbackMonitor?.IsPrimaryPlaying(_profile.Primary.SourceKey) == true;
            bool primaryInactive =
                silentHeld && !mediaPlaying;
            _logger?.Info(
                $"Primary {_profile.Primary.SourceKey} level = {primaryDb:0.0} dB, silentHeld={silentHeld}");
            Debug.WriteLine(
             $"Primary {_profile.Primary.SourceKey} " +
             $"level={primaryDb:0.0} dB, " +
             $"audioSilentHeld={silentHeld}, " +
             $"mediaPlaying={mediaPlaying}, " +
             $"primaryInactive={primaryInactive}");

            if (_fader.IsBusy)
            {
                _logger?.Info("Skipping primary evaluation (fade in progress)");
                return;
            }

            if (_state == AudioBehaviorState.Monitoring && !mediaPlaying)
                ActivateSecondary();
            else if (_state == AudioBehaviorState.SecondaryActive && mediaPlaying)
                DeactivateSecondary();
        }

        private void ActivateSecondary()
        {
            if (_state == AudioBehaviorState.SecondaryActive)
                return;

            SetState(AudioBehaviorState.SecondaryActive);

            _logger?.Info(
                $"Activating secondary: {_profile.Secondary.SourceKey}");

            StartFade(
            _profile.Secondary.SourceKey,
            _profile.SecondaryMaxVolume,
            _profile.FadeInMs);
        }


        private void DeactivateSecondary()
        {
            if (_state != AudioBehaviorState.SecondaryActive)
                return;

            SetState(AudioBehaviorState.Monitoring);

            _logger?.Info(
                $"Deactivating secondary: {_profile.Secondary.SourceKey}");

            StartFade(
                _profile.Secondary.SourceKey,
                _profile.SecondaryMinVolume,   // ✅ FIX
                _profile.FadeOutMs);
        }

        private void StartFade(string key, float target, int duration)
        {
            _fadeCts?.Cancel();
            _fadeCts = new CancellationTokenSource();

            _ = _fader.FadeToAsync(
                key,
                target,
                duration,
                _fadeCts.Token);
        }

        private void SetState(AudioBehaviorState newState)
        {
            if (_state == newState)
                return;

            _state = newState;
            StateChanged?.Invoke(_state);

        }

        public void UpdateProfile(
             AudioBehaviorProfile profile,
             AudioSourceRegistry registry)
        {
            _profile = profile;
            _registry = registry;
        }
    }
}
