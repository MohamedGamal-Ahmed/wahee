using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Wahee.Core.Interfaces;

namespace Wahee.Infrastructure.Services
{
    /// <summary>
    /// Service that monitors prayer times and triggers Adhan notifications.
    /// The UI layer subscribes to the AdhanTriggered event to show notifications and play audio.
    /// </summary>
    public class AdhanNotificationService : IDisposable
    {
        private readonly IPrayerTimeService _prayerTimeService;
        private readonly Timer _checkTimer;
        private Dictionary<string, string>? _todayPrayerTimes;
        private string _lastPlayedPrayer = string.Empty;
        private DateTime _lastPlayedDate = DateTime.MinValue;
        private bool _isEnabled = true;
        private string _currentCity = "Cairo";
        private string _currentCountry = "Egypt";

        private static readonly Dictionary<string, string> PrayerNamesAr = new()
        {
            { "Fajr", "الفجر" },
            { "Dhuhr", "الظهر" },
            { "Asr", "العصر" },
            { "Maghrib", "المغرب" },
            { "Isha", "العشاء" }
        };

        /// <summary>
        /// Raised when it's time for Adhan. The string parameter is the Arabic prayer name.
        /// </summary>
        public event EventHandler<string>? AdhanTriggered;

        public AdhanNotificationService(IPrayerTimeService prayerTimeService)
        {
            _prayerTimeService = prayerTimeService;
            
            // Check every 30 seconds
            _checkTimer = new Timer(CheckPrayerTime, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public string CurrentLocation => $"{_currentCity}, {_currentCountry}";

        public async Task InitializeAsync()
        {
            try
            {
                var location = await _prayerTimeService.GetLocationByIpAsync();
                _currentCity = location.City;
                _currentCountry = location.Country;
                _todayPrayerTimes = await _prayerTimeService.GetPrayerTimesAsync(_currentCity, _currentCountry);
                
                System.Diagnostics.Debug.WriteLine($"Adhan service initialized for {_currentCity}, {_currentCountry}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing Adhan service: {ex.Message}");
            }
        }

        private void CheckPrayerTime(object? state)
        {
            if (!_isEnabled || _todayPrayerTimes == null) return;

            var now = DateTime.Now;
            var currentTime = now.ToString("HH:mm");

            // Reset if it's a new day
            if (now.Date != _lastPlayedDate)
            {
                _lastPlayedPrayer = string.Empty;
                _lastPlayedDate = now.Date;
                
                // Refresh prayer times for new day
                Task.Run(async () => await InitializeAsync());
            }

            foreach (var prayer in PrayerNamesAr.Keys)
            {
                if (_todayPrayerTimes.TryGetValue(prayer, out string? prayerTime))
                {
                    // Check if current time matches prayer time
                    if (prayerTime == currentTime && _lastPlayedPrayer != prayer)
                    {
                        _lastPlayedPrayer = prayer;
                        var prayerNameAr = PrayerNamesAr.GetValueOrDefault(prayer, prayer);
                        
                        System.Diagnostics.Debug.WriteLine($"Adhan triggered for {prayerNameAr}");
                        
                        // Raise event - UI will handle notification and audio
                        AdhanTriggered?.Invoke(this, prayerNameAr);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the current prayer times for display purposes.
        /// </summary>
        public Dictionary<string, string>? GetTodayPrayerTimes() => _todayPrayerTimes;

        /// <summary>
        /// Gets the Arabic prayer names mapping.
        /// </summary>
        public static Dictionary<string, string> GetPrayerNamesArabic() => PrayerNamesAr;

        public void Dispose()
        {
            _checkTimer?.Dispose();
        }
    }
}
