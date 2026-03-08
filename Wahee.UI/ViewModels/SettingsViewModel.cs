// Task Summary:
// - Added manual location settings (City/Country) and seasonal mode (Auto/Winter/Summer).
// - Exposed IsManualLocationEnabled for settings UI enable/disable logic.
using System.Windows.Input;
using Wahee.Core.Interfaces;
using Wahee.Core.ViewModels;

namespace Wahee.UI.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsService _settingsService;

        private bool _adhanEnabled = true;
        public bool AdhanEnabled
        {
            get => _adhanEnabled;
            set
            {
                if (SetProperty(ref _adhanEnabled, value))
                {
                    _ = SaveSettingAsync("AdhanEnabled", value.ToString());
                }
            }
        }

        private bool _startWithWindows;
        public bool StartWithWindows
        {
            get => _startWithWindows;
            set
            {
                if (SetProperty(ref _startWithWindows, value))
                {
                    _ = SaveSettingAsync("StartWithWindows", value.ToString());
                }
            }
        }

        private bool _minimizeToTray;
        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                if (SetProperty(ref _minimizeToTray, value))
                {
                    _ = SaveSettingAsync("MinimizeToTray", value.ToString());
                }
            }
        }

        private bool _azkarReminder;
        public bool AzkarReminder
        {
            get => _azkarReminder;
            set
            {
                if (SetProperty(ref _azkarReminder, value))
                {
                    _ = SaveSettingAsync("AzkarReminder", value.ToString());
                }
            }
        }

        private bool _autoDetectLocation = true;
        public bool AutoDetectLocation
        {
            get => _autoDetectLocation;
            set
            {
                if (SetProperty(ref _autoDetectLocation, value))
                {
                    OnPropertyChanged(nameof(IsManualLocationEnabled));
                    _ = SaveSettingAsync("AutoDetectLocation", value.ToString());
                }
            }
        }

        public bool IsManualLocationEnabled => !AutoDetectLocation;

        private string _manualCity = "Cairo";
        public string ManualCity
        {
            get => _manualCity;
            set
            {
                if (SetProperty(ref _manualCity, value))
                {
                    _ = SaveSettingAsync("ManualCity", value);
                }
            }
        }

        private string _manualCountry = "Egypt";
        public string ManualCountry
        {
            get => _manualCountry;
            set
            {
                if (SetProperty(ref _manualCountry, value))
                {
                    _ = SaveSettingAsync("ManualCountry", value);
                }
            }
        }

        private string _seasonMode = "Auto";
        public string SeasonMode
        {
            get => _seasonMode;
            set
            {
                if (SetProperty(ref _seasonMode, value))
                {
                    _ = SaveSettingAsync("SeasonMode", value);
                }
            }
        }

        private string _selectedTheme = "Dark";
        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    _ = SaveSettingAsync("Theme", value);
                }
            }
        }

        public ICommand LoadSettingsCommand { get; }

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadSettingsCommand = new AsyncRelayCommand(_ => LoadSettingsAsync());
        }

        public async Task LoadSettingsAsync()
        {
            try
            {
                var adhan = await _settingsService.GetSettingAsync("AdhanEnabled");
                _adhanEnabled = !bool.TryParse(adhan, out var adhanValue) || adhanValue;

                var startup = await _settingsService.GetSettingAsync("StartWithWindows");
                _startWithWindows = bool.TryParse(startup, out var startupValue) && startupValue;

                var tray = await _settingsService.GetSettingAsync("MinimizeToTray");
                _minimizeToTray = bool.TryParse(tray, out var t) && t;

                var azkar = await _settingsService.GetSettingAsync("AzkarReminder");
                _azkarReminder = bool.TryParse(azkar, out var a) && a;

                var location = await _settingsService.GetSettingAsync("AutoDetectLocation");
                _autoDetectLocation = !bool.TryParse(location, out var l) || l;

                _manualCity = await _settingsService.GetSettingAsync("ManualCity") ?? "Cairo";
                _manualCountry = await _settingsService.GetSettingAsync("ManualCountry") ?? "Egypt";
                _seasonMode = await _settingsService.GetSettingAsync("SeasonMode") ?? "Auto";

                var theme = await _settingsService.GetSettingAsync("Theme");
                _selectedTheme = theme ?? "Dark";

                OnPropertyChanged(nameof(AdhanEnabled));
                OnPropertyChanged(nameof(StartWithWindows));
                OnPropertyChanged(nameof(MinimizeToTray));
                OnPropertyChanged(nameof(AzkarReminder));
                OnPropertyChanged(nameof(AutoDetectLocation));
                OnPropertyChanged(nameof(IsManualLocationEnabled));
                OnPropertyChanged(nameof(ManualCity));
                OnPropertyChanged(nameof(ManualCountry));
                OnPropertyChanged(nameof(SeasonMode));
                OnPropertyChanged(nameof(SelectedTheme));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Load error: {ex.Message}");
            }
        }

        private async Task SaveSettingAsync(string key, string value)
        {
            try
            {
                await _settingsService.SaveSettingAsync(key, value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SettingsViewModel] Save error: {ex.Message}");
            }
        }
    }
}
