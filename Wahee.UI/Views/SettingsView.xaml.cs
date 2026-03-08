using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Wahee.Infrastructure.Services;

namespace Wahee.UI.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly string _settingsPath;
        private AppSettings _settings = new();

        public SettingsView()
        {
            InitializeComponent();
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Wahee", "settings.json");
            
            Loaded += SettingsView_Loaded;
        }

        private void SettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            ApplySettings();
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading settings: {ex.Message}");
                _settings = new AppSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                string? dir = Path.GetDirectoryName(_settingsPath);
                if (dir != null && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private void ApplySettings()
        {
            AdhanToggle.IsChecked = _settings.AdhanEnabled;
            AzkarReminderToggle.IsChecked = _settings.AzkarReminderEnabled;
            StartupToggle.IsChecked = _settings.StartWithWindows;
            TrayToggle.IsChecked = _settings.MinimizeToTray;
            AutoLocationToggle.IsChecked = _settings.AutoLocation;
        }

        private void AdhanToggle_Changed(object sender, RoutedEventArgs e)
        {
            _settings.AdhanEnabled = AdhanToggle.IsChecked == true;
            SaveSettings();

            // Update Adhan service
            try
            {
                var adhanService = App.ServiceProvider?.GetService<AdhanNotificationService>();
                if (adhanService != null)
                {
                    adhanService.IsEnabled = _settings.AdhanEnabled;
                }
            }
            catch { }
        }

        private void StartupToggle_Changed(object sender, RoutedEventArgs e)
        {
            _settings.StartWithWindows = StartupToggle.IsChecked == true;
            SaveSettings();
            SetStartupWithWindows(_settings.StartWithWindows);
        }

        private void SetStartupWithWindows(bool enable)
        {
            try
            {
                string appPath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                
                if (key != null)
                {
                    if (enable)
                    {
                        key.SetValue("Wahee", $"\"{appPath}\"");
                    }
                    else
                    {
                        key.DeleteValue("Wahee", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error setting startup: {ex.Message}");
            }
        }

        private void VisitWebsite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/mo-gamal/wahee",
                    UseShellExecute = true
                });
            }
            catch { }
        }

        // Settings model
        private class AppSettings
        {
            public bool AdhanEnabled { get; set; } = true;
            public bool AzkarReminderEnabled { get; set; } = false;
            public bool StartWithWindows { get; set; } = false;
            public bool MinimizeToTray { get; set; } = true;
            public bool AutoLocation { get; set; } = true;
            public string DefaultReciter { get; set; } = "afs"; // Al-Afasy
        }
    }
}
