// Task Summary:
// - Compact horizontal prayer widget layout support.
// - Added pin/unpin (Topmost) toggle for always-on-top behavior.
// - Improved time parsing to handle API formats like "HH:mm (EET)" safely.
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Wahee.Core.Constants;
using Wahee.Core.Interfaces;

namespace Wahee.UI.Widgets
{
    public partial class PrayerWidget : WaheeWidgetBase
    {
        private readonly IPrayerTimeService? _prayerService;
        private Dictionary<string, string>? _currentPrayerTimes;
        private readonly DispatcherTimer _updateTimer;
        private bool _isPinned;

        public PrayerWidget()
        {
            InitializeComponent();
            _prayerService = App.ServiceProvider?.GetService<IPrayerTimeService>();

            _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _updateTimer.Tick += UpdateTimer_Tick;

            IsVisibleChanged += Window_IsVisibleChanged;
        }

        protected override void InitializeWidget()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            this.Left = screenWidth - this.Width - 30;
            this.Top = 80;

            if (Resources["FadeIn"] is System.Windows.Media.Animation.Storyboard sb)
            {
                sb.Begin(this);
            }

            _updateTimer.Start();
            ApplyPinState();
        }

        protected override async void LoadWidgetData()
        {
            if (_prayerService == null) return;

            try
            {
                var location = await _prayerService.GetLocationByIpAsync();
                LocationTxt.Text = $"{location.City}, {location.Country}";
                _currentPrayerTimes = await _prayerService.GetPrayerTimesAsync(location.City, location.Country);

                if (_currentPrayerTimes != null)
                {
                    UpdatePrayerList();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading prayer data: {ex.Message}");
                LocationTxt.Text = "تعذر تحميل الموقع";
            }
        }

        private void UpdatePrayerList()
        {
            if (_currentPrayerTimes == null) return;

            var prayers = new List<PrayerInfo>();

            for (int i = 0; i < PrayerConstants.Keys.Length; i++)
            {
                var key = PrayerConstants.Keys[i];
                if (_currentPrayerTimes.TryGetValue(key, out var rawTime))
                {
                    prayers.Add(new PrayerInfo
                    {
                        NameAr = PrayerConstants.NamesAr[i],
                        NameEn = key,
                        Time = ExtractPrayerTime(rawTime)
                    });
                }
            }

            PrayerListControl.ItemsSource = prayers;
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (_currentPrayerTimes == null) return;

            var now = DateTime.Now;
            DateTime? nextPrayerTime = null;
            string nextPrayerName = string.Empty;

            for (int i = 0; i < PrayerConstants.Keys.Length; i++)
            {
                var key = PrayerConstants.Keys[i];
                if (_currentPrayerTimes.TryGetValue(key, out var rawTime) &&
                    DateTime.TryParseExact(ExtractPrayerTime(rawTime), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTime))
                {
                    var fullPrayerTime = now.Date.Add(parsedTime.TimeOfDay);
                    if (fullPrayerTime > now)
                    {
                        nextPrayerTime = fullPrayerTime;
                        nextPrayerName = PrayerConstants.NamesAr[i];
                        break;
                    }
                }
            }

            if (nextPrayerTime == null && _currentPrayerTimes.TryGetValue(PrayerConstants.Keys[0], out var nextDayFajrRaw) &&
                DateTime.TryParseExact(ExtractPrayerTime(nextDayFajrRaw), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fajrParsedTime))
            {
                nextPrayerTime = now.Date.AddDays(1).Add(fajrParsedTime.TimeOfDay);
                nextPrayerName = PrayerConstants.NamesAr[0];
            }

            if (nextPrayerTime != null)
            {
                var diff = nextPrayerTime.Value - now;
                if (diff.TotalSeconds > -1 && diff.TotalSeconds <= 1)
                {
                    _prayerService?.PlayAdhan();
                }

                CountdownTxt.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", diff.Hours, diff.Minutes, diff.Seconds);
                NextPrayerTxt.Text = $"الصلاة القادمة: {nextPrayerName}";
            }
        }

        private static string ExtractPrayerTime(string rawTime)
        {
            if (string.IsNullOrWhiteSpace(rawTime)) return "00:00";

            var trimmed = rawTime.Trim();
            if (trimmed.Length >= 5)
            {
                var candidate = trimmed.Substring(0, 5);
                if (DateTime.TryParseExact(candidate, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    return candidate;
                }
            }

            return trimmed;
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            _isPinned = !_isPinned;
            ApplyPinState();
        }

        private void ApplyPinState()
        {
            Topmost = _isPinned;
            PinButton.Content = _isPinned ? "مثبّت" : "تثبيت";
            PinButton.Background = _isPinned ? (System.Windows.Media.Brush)FindResource("AccentBrush") : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x18, 0xFF, 0xFF, 0xFF));
            PinButton.Foreground = _isPinned ? System.Windows.Media.Brushes.White : (System.Windows.Media.Brush)FindResource("TextForegroundBrush");
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
                _updateTimer.Start();
            else
                _updateTimer.Stop();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _updateTimer.Stop();
            e.Cancel = true;
            Hide();
        }
    }

    public class PrayerInfo
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
    }
}
