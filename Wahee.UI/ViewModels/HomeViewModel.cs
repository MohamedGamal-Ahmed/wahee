// Task Summary:
// - Phase 2 / Task 1: Added HomeViewModel for greeting + prayer countdown state.
using System.Globalization;
using System.Windows.Input;
using Wahee.Core.Constants;
using Wahee.Core.Interfaces;
using Wahee.Core.ViewModels;

namespace Wahee.UI.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly IPrayerTimeService _prayerTimeService;

        private string _greeting = string.Empty;
        public string Greeting { get => _greeting; set => SetProperty(ref _greeting, value); }

        private string _locationText = "جاري تحديد الموقع...";
        public string LocationText { get => _locationText; set => SetProperty(ref _locationText, value); }

        private string _nextPrayerName = string.Empty;
        public string NextPrayerName { get => _nextPrayerName; set => SetProperty(ref _nextPrayerName, value); }

        private string _countdown = "--:--:--";
        public string Countdown { get => _countdown; set => SetProperty(ref _countdown, value); }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        private string _errorMessage = string.Empty;
        public string ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

        private Dictionary<string, string> _prayerTimes = new();

        public ICommand RetryCommand { get; }

        public HomeViewModel(IPrayerTimeService prayerTimeService)
        {
            _prayerTimeService = prayerTimeService;
            RetryCommand = new AsyncRelayCommand(_ => LoadPrayerTimesAsync());
            SetGreeting();
        }

        private void SetGreeting()
        {
            var hour = DateTime.Now.Hour;
            Greeting = hour switch
            {
                >= 5 and < 12 => "صباح الخير، تقبل الله طاعاتكم",
                >= 12 and < 17 => "السلام عليكم، يومكم مبارك",
                >= 17 and < 21 => "مساء الخير، جعل الله حياتكم سكينة",
                _ => "طاب مساؤكم، لا تنسوا قيام الليل"
            };
        }

        public async Task LoadPrayerTimesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                LocationText = "جاري تحديد الموقع...";

                var location = await _prayerTimeService.GetLocationByIpAsync();
                LocationText = $"{location.City}, {location.Country}";
                _prayerTimes = await _prayerTimeService.GetPrayerTimesAsync(location.City, location.Country);
                UpdateCountdown();
            }
            catch (Exception ex)
            {
                LocationText = string.Empty;
                ErrorMessage = "تعذر تحديد الموقع — اضغط للمحاولة مرة أخرى";
                System.Diagnostics.Debug.WriteLine($"[HomeViewModel] Prayer load error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void UpdateCountdown()
        {
            if (_prayerTimes.Count == 0)
            {
                NextPrayerName = "";
                Countdown = "--:--:--";
                return;
            }

            var now = DateTime.Now;

            for (int i = 0; i < PrayerConstants.Keys.Length; i++)
            {
                var key = PrayerConstants.Keys[i];
                if (_prayerTimes.TryGetValue(key, out var timeStr) &&
                    DateTime.TryParseExact(timeStr, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    var prayerTime = now.Date.Add(parsed.TimeOfDay);
                    if (prayerTime > now)
                    {
                        var diff = prayerTime - now;
                        NextPrayerName = $"الصلاة القادمة: {PrayerConstants.NamesAr[i]}";
                        Countdown = $"{diff.Hours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2} حتى الأذان";
                        return;
                    }
                }
            }

            NextPrayerName = "في انتظار صلاة الفجر الغد";
            Countdown = "--:--:--";
        }
    }
}
