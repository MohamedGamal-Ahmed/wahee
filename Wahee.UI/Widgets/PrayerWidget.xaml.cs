using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Wahee.Core.Interfaces;

namespace Wahee.UI.Widgets
{
    public partial class PrayerWidget : WaheeWidgetBase
    {
        private readonly IPrayerTimeService? _prayerService;
        private Dictionary<string, string>? _currentPrayerTimes;
        private readonly DispatcherTimer _updateTimer;

        public PrayerWidget()
        {
            InitializeComponent();
            _prayerService = App.ServiceProvider?.GetService<IPrayerTimeService>();
            
            _updateTimer = new DispatcherTimer();
            _updateTimer.Interval = TimeSpan.FromSeconds(1);
            _updateTimer.Tick += UpdateTimer_Tick;
        }

        protected override void InitializeWidget()
        {
            // Initial positioning
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            this.Left = screenWidth - this.Width - 100;
            this.Top = 100;

            if (Resources["FadeIn"] is System.Windows.Media.Animation.Storyboard sb)
            {
                sb.Begin(this);
            }
            
            _updateTimer.Start();
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

            var prayers = new List<PrayerInfo>
            {
                new PrayerInfo { NameAr = "الفجر", NameEn = "Fajr", Time = _currentPrayerTimes["Fajr"] },
                new PrayerInfo { NameAr = "الظهر", NameEn = "Dhuhr", Time = _currentPrayerTimes["Dhuhr"] },
                new PrayerInfo { NameAr = "العصر", NameEn = "Asr", Time = _currentPrayerTimes["Asr"] },
                new PrayerInfo { NameAr = "المغرب", NameEn = "Maghrib", Time = _currentPrayerTimes["Maghrib"] },
                new PrayerInfo { NameAr = "العشاء", NameEn = "Isha", Time = _currentPrayerTimes["Isha"] }
            };

            PrayerListControl.ItemsSource = prayers;
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (_currentPrayerTimes == null) return;

            var now = DateTime.Now;
            string[] prayerKeys = { "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha" };
            string[] prayerNamesAr = { "الفجر", "الظهر", "العصر", "المغرب", "العشاء" };

            DateTime? nextPrayerTime = null;
            string nextPrayerName = "";

            for (int i = 0; i < prayerKeys.Length; i++)
            {
                if (_currentPrayerTimes.TryGetValue(prayerKeys[i], out string? timeStr))
                {
                    var pTime = DateTime.ParseExact(timeStr, "HH:mm", null);
                    var fullPTime = now.Date.Add(pTime.TimeOfDay);

                    if (fullPTime > now)
                    {
                        nextPrayerTime = fullPTime;
                        nextPrayerName = prayerNamesAr[i];
                        break;
                    }
                }
            }

            // If no next prayer today, it's Fajr tomorrow
            if (nextPrayerTime == null && _currentPrayerTimes.TryGetValue("Fajr", out string? fajrStr))
            {
                var pTime = DateTime.ParseExact(fajrStr, "HH:mm", null);
                nextPrayerTime = now.Date.AddDays(1).Add(pTime.TimeOfDay);
                nextPrayerName = "الفجر";
            }

            if (nextPrayerTime != null)
            {
                var diff = nextPrayerTime.Value - now;
                
                // If it's time for prayer (within 1 second)
                if (diff.TotalSeconds > -1 && diff.TotalSeconds <= 1)
                {
                    _prayerService?.PlayAdhan();
                }

                CountdownTxt.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", diff.Hours, diff.Minutes, diff.Seconds);
                NextPrayerTxt.Text = $"الصلاة القادمة: {nextPrayerName}";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }

    public class PrayerInfo
    {
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string Time { get; set; } = "";
    }
}
