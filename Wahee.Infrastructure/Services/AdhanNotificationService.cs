// Task Summary:
// - Phase 2 / Task 6: Unified prayer key naming through PrayerConstants.
using Microsoft.Extensions.DependencyInjection;
using Wahee.Core.Constants;
using Wahee.Core.Interfaces;

namespace Wahee.Infrastructure.Services
{
    /// <summary>
    /// Service that monitors prayer times and triggers Adhan notifications.
    /// The UI layer subscribes to the AdhanTriggered event to show notifications and play audio.
    /// </summary>
    public class AdhanNotificationService : IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly Timer _checkTimer;
        private Dictionary<string, string>? _todayPrayerTimes;
        private string _lastPlayedPrayer = string.Empty;
        private DateTime _lastPlayedDate = DateTime.MinValue;
        private bool _isEnabled = true;
        private string _currentCity = "Cairo";
        private string _currentCountry = "Egypt";
        private bool _isRefreshing;

        /// <summary>
        /// Raised when it's time for Adhan. The string parameter is the Arabic prayer name.
        /// </summary>
        public event EventHandler<string>? AdhanTriggered;

        public AdhanNotificationService(IServiceScopeFactory scopeFactory)
        {
            // FIXED: Captive dependency in DI
            _scopeFactory = scopeFactory;
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
            await RefreshPrayerTimesAsync();
        }

        private async Task RefreshPrayerTimesAsync()
        {
            if (_isRefreshing) return;

            try
            {
                _isRefreshing = true;

                using var scope = _scopeFactory.CreateScope();
                var prayerTimeService = scope.ServiceProvider.GetRequiredService<IPrayerTimeService>();

                var location = await prayerTimeService.GetLocationByIpAsync();
                _currentCity = location.City;
                _currentCountry = location.Country;
                _todayPrayerTimes = await prayerTimeService.GetPrayerTimesAsync(_currentCity, _currentCountry);

                System.Diagnostics.Debug.WriteLine($"Adhan service initialized for {_currentCity}, {_currentCountry}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing Adhan service: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
            }
        }

        private void CheckPrayerTime(object? state)
        {
            if (!_isEnabled || _todayPrayerTimes == null) return;

            var now = DateTime.Now;
            var currentTime = now.ToString("HH:mm");

            if (now.Date != _lastPlayedDate)
            {
                _lastPlayedPrayer = string.Empty;
                _lastPlayedDate = now.Date;
                _ = RefreshPrayerTimesAsync();
            }

            foreach (var prayerKey in PrayerConstants.Keys)
            {
                if (_todayPrayerTimes.TryGetValue(prayerKey, out string? prayerTime) && prayerTime == currentTime && _lastPlayedPrayer != prayerKey)
                {
                    _lastPlayedPrayer = prayerKey;
                    var prayerNameAr = PrayerConstants.KeyToArabic.GetValueOrDefault(prayerKey, prayerKey);

                    System.Diagnostics.Debug.WriteLine($"Adhan triggered for {prayerNameAr}");
                    AdhanTriggered?.Invoke(this, prayerNameAr);
                    break;
                }
            }
        }

        public Dictionary<string, string>? GetTodayPrayerTimes() => _todayPrayerTimes;

        public static Dictionary<string, string> GetPrayerNamesArabic() => new(PrayerConstants.KeyToArabic);

        public void Dispose()
        {
            _checkTimer?.Dispose();
        }
    }
}
