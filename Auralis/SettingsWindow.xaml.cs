using Auralis.Audio.Wasapi;
using Auralis.Core.Engine;
using Auralis.Core.Models;
using Auralis.Core.Persistence;
using Auralis.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;


namespace Auralis
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly AudioBehaviorCoordinator _coordinator;
        private readonly IAudioBehaviorProfileStore _store;
        private readonly AudioBehaviorProfile _currentProfile;
        private AudioSourceRegistry _registry;
        public SettingsWindow(AudioBehaviorCoordinator coordinator, IAudioBehaviorProfileStore store, AudioBehaviorProfile currentProfile, AudioSourceRegistry registry)
        {
            InitializeComponent();
            _coordinator = coordinator;
            _store = store;
            _currentProfile = currentProfile;
            _registry = registry;

            LoadSources();
            PreselectProcesses();
        }

        private void LoadSources()
        {
            using var enumerator = new WasapiAudioSourceEnumerator();
            var sources = enumerator.GetActiveSources();

            PrimaryCombo.ItemsSource = sources;
            SecondaryCombo.ItemsSource = sources;
        }

        private void OnApplyClicked(object sender, RoutedEventArgs e)
        {
            if (PrimaryCombo.SelectedItem is not AudioSource primary ||
                SecondaryCombo.SelectedItem is not AudioSource secondary)
            {
                System.Windows.MessageBox.Show("Please select both Primary and Secondary sources.");
                return;
            }
            
            var profile = new AudioBehaviorProfile
            {
                Primary = primary,
                Secondary = secondary,
                SecondaryMaxVolume = 0.5f,
                SecondaryMinVolume = 0.0f
            };
            _registry.ResolveAndRegister(profile.Primary);
            _registry.ResolveAndRegister(profile.Secondary);
            _store.SaveProfile(profile);

            // ⚠️ No registry, no sessions, no WASAPI here
            _coordinator.UpdateProfile(profile, _registry);

            Close();
        }


      



        private void PreselectProcesses()
        {
            if (PrimaryCombo.ItemsSource is not IEnumerable<AudioSource> processes)
                return;

            PrimaryCombo.SelectedItem = processes
                .FirstOrDefault(p =>
                    string.Equals(
                        p.SourceKey,
                        _currentProfile.Primary.SourceKey,
                        StringComparison.OrdinalIgnoreCase));

            SecondaryCombo.SelectedItem = processes
                .FirstOrDefault(p =>
                    string.Equals(
                        p.SourceKey,
                        _currentProfile.Secondary.SourceKey,
                        StringComparison.OrdinalIgnoreCase));
        }
    }
}
