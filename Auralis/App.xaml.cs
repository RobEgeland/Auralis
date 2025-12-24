using Auralis.Audio.Fake;
using Auralis.Audio.Wasapi;
using Auralis.Core.Engine;
using Auralis.Core.Logging;
using Auralis.Core.Persistence;
using Auralis.Infrastructure.Persistence;
using Auralis.Logging;
using Auralis.Services;
using Auralis.Tray;
using Microsoft.Win32;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace Auralis
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private TrayIconService? _tray;
        private AudioBehaviorCoordinator? _coordinator;
        private IAuralisLogger? _logger;
        private IAudioBehaviorProfileStore? _profileStore;
        private AudioSourceRegistry? _registry;


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _logger = new FileAuralisLogger();
            _logger.Info("Auralis started");
            _registry = new AudioSourceRegistry();

            var meter = new WasapiAudioMeter();
            var fader = new WasapiFadeEngine(_logger, _registry);
            _profileStore = new JsonAudioBehaviorProfileStore();

            var profile = _profileStore.LoadProfile()
                          ?? AudioBehaviorProfileFactory.CreateDefaultProfile();


            _coordinator = new AudioBehaviorCoordinator(meter, fader, profile, _registry,  _logger);
            _coordinator.StateChanged += OnStateChanged;

            _tray = new TrayIconService();

            _tray.EnableStudyModeRequested += OnEnable;
            _tray.DisableStudyModeRequested += OnDisable;
            _tray.ExitRequested += OnExit;
            _tray.SettingsRequested += OnSettingsRequested;

        }
        private void OnEnable()
        {
            _logger?.Info("Behavior ENABLED");
            _coordinator?.Enable();
        }

        private void OnDisable()
        {
            _logger?.Info("Behavior DISABLED");
            _coordinator?.Disable();
        }

        private void OnExit()
        {
            _logger?.Info("Auralis exiting");
            _tray?.Dispose();
            _coordinator?.Disable();
            Shutdown();
        }
        private void OnSettingsRequested()
        {
            var window = new SettingsWindow(
               _coordinator!,
               _profileStore!,
               _coordinator!.CurrentProfile,
               _registry);

            window.Show();
        }

        private void OnStateChanged(AudioBehaviorState state)
        {
            _logger?.Info($"State changed → {state}");
            _tray?.UpdateStatus(state.ToString());
        }
    }



}
