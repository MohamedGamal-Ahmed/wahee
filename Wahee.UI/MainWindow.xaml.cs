using System.Windows;
using Wahee.Core.Interfaces;
using Wahee.UI.Widgets;
using System.Drawing;

namespace Wahee.UI
{
    public partial class MainWindow : Window
    {
        private readonly IWallpaperService _wallpaperService;
        private readonly IQuranDataService _quranDataService;
        private readonly IPrayerTimeService _prayerTimeService;
        private readonly System.Windows.Media.MediaPlayer _mediaPlayer = new System.Windows.Media.MediaPlayer();
        private readonly System.Windows.Threading.DispatcherTimer _prayerTimer;
        private AyahWidget? _ayahWidget;
        private PrayerWidget? _prayerWidget;
        private FloatingRadio? _floatingRadio;
        private Dictionary<string, string>? _currentPrayerTimes;
        private System.Windows.Forms.NotifyIcon? _notifyIcon;

        public MainWindow(IWallpaperService wallpaperService, IQuranDataService quranDataService, IPrayerTimeService prayerTimeService)
        {
            _wallpaperService = wallpaperService;
            _quranDataService = quranDataService;
            _prayerTimeService = prayerTimeService;
            InitializeComponent();
            InitializePrayerTimes();
            
            _prayerTimer = new System.Windows.Threading.DispatcherTimer();
            _prayerTimer.Interval = TimeSpan.FromSeconds(1);
            _prayerTimer.Tick += PrayerTimer_Tick;
            _prayerTimer.Start();
            SetGreeting();
            InitializeSystemTray();
            StateChanged += MainWindow_StateChanged;
        }

        private void SetGreeting()
        {
            var hour = DateTime.Now.Hour;
            string greeting;
            if (hour >= 5 && hour < 12) greeting = "صباح الخير، تقبل الله طاعاتكم";
            else if (hour >= 12 && hour < 17) greeting = "السلام عليكم، يومكم مبارك";
            else if (hour >= 17 && hour < 21) greeting = "مساء الخير، جعل الله حياتكم سكينة";
            else greeting = "طاب مساؤكم، لا تنسوا قيام الليل";

            GreetingTxt.Text = greeting;
        }

        private async void InitializePrayerTimes()
        {
            var location = await _prayerTimeService.GetLocationByIpAsync();
            LocationTxt.Text = $"{location.City}, {location.Country}";
            
            _currentPrayerTimes = await _prayerTimeService.GetPrayerTimesAsync(location.City, location.Country);
        }

        private void PrayerTimer_Tick(object? sender, EventArgs e)
        {
            if (_currentPrayerTimes == null) return;

            var now = DateTime.Now;
            var currentTimeStrings = now.ToString("HH:mm");
            
            // Very simple logic to find next prayer
            string[] prayerNames = { "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha" };
            string[] prayerNamesAr = { "الفجر", "الظهر", "العصر", "المغرب", "العشاء" };
            
            for (int i = 0; i < prayerNames.Length; i++)
            {
                if (_currentPrayerTimes.TryGetValue(prayerNames[i], out string? timeStr))
                {
                    var prayerTime = DateTime.ParseExact(timeStr, "HH:mm", null);
                    if (prayerTime > now)
                    {
                        var diff = prayerTime - now;
                        NextPrayerNameTxt.Text = $"الصلاة القادمة: {prayerNamesAr[i]}";
                        CountdownTxt.Text = $"{diff.Hours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2} حتى الأذان";
                        return;
                    }
                }
            }

            NextPrayerNameTxt.Text = "في انتظار صلاة الفجر الغد";
            CountdownTxt.Text = "--:--:--";
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag != null)
            {
                string target = element.Tag.ToString()!;
                HomePage.Visibility = target == "Home" ? Visibility.Visible : Visibility.Collapsed;
                RadiosPage.Visibility = target == "Radios" ? Visibility.Visible : Visibility.Collapsed;
                QuranPage.Visibility = target == "Quran" ? Visibility.Visible : Visibility.Collapsed;
                MushafPage.Visibility = target == "Mushaf" ? Visibility.Visible : Visibility.Collapsed;
                AzkarPage.Visibility = target == "Azkar" ? Visibility.Visible : Visibility.Collapsed;
                SettingsPage.Visibility = target == "Settings" ? Visibility.Visible : Visibility.Collapsed;
                AboutPageContainer.Visibility = target == "About" ? Visibility.Visible : Visibility.Collapsed;

                if (target == "Radios" && RadiosPageContainer.Content == null)
                {
                    RadiosPageContainer.Content = new Views.RadioCategoryTabControl();
                }

                if (target == "Quran" && QuranPageContainer.Content == null)
                {
                    QuranPageContainer.Content = new Views.QuranView();
                }

                if (target == "Mushaf" && MushafPageContainer.Content == null)
                {
                    MushafPageContainer.Content = new Views.MushafView();
                }

                if (target == "Azkar" && AzkarPageContainer.Content == null)
                {
                    AzkarPageContainer.Content = new Views.AzkarView();
                }

                if (target == "Settings" && SettingsPageContainer.Content == null)
                {
                    SettingsPageContainer.Content = new Views.SettingsView();
                }

                if (target == "About" && AboutPageContainer.Content == null)
                {
                    AboutPageContainer.Content = new Views.AboutPage();
                }
                
                HeaderTitle.Text = target switch {
                    "Home" => "وحي (Wahee)",
                    "Radios" => "إذاعات الوحي",
                    "Quran" => "البحث في القرآن",
                    "Mushaf" => "المصحف الشريف",
                    "Azkar" => "الأذكار",
                    "Settings" => "الإعدادات",
                    "About" => "عن البرنامج",
                    _ => "وحي (Wahee)"
                };

                HeaderSubtitle.Text = target switch {
                    "Home" => "تطبيقات وإضافات إسلامية لسطح المكتب",
                    "Radios" => "بث مباشر من موقع MP3Quran.net",
                    "Quran" => "بحث وتفسير الآيات الكريمة",
                    "Mushaf" => "تصفح المصحف الورقي (مصحف المدينة)",
                    "Azkar" => "أذكار الصباح والمساء",
                    "Settings" => "تخصيص تجربة الاستخدام والمظهر",
                    "About" => "حول نظام وحي وحقوق الاستخدام",
                    _ => "وحي (Wahee)"
                };
            }
        }

        // Radio functionality moved to RadioCategoryTabControl

        private void AyahToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_ayahWidget == null)
            {
                _ayahWidget = new AyahWidget();
            }
            _ayahWidget.Show();
        }

        private void AyahToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _ayahWidget?.Hide();
        }

        private void PrayerWidgetToggle_Checked(object sender, RoutedEventArgs e)
        {
            if (_prayerWidget == null)
            {
                _prayerWidget = new PrayerWidget();
            }
            _prayerWidget.Show();
        }

        private void PrayerWidgetToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _prayerWidget?.Hide();
        }

        private void FloatingRadioToggle_Checked(object sender, RoutedEventArgs e)
        {
            // Stop main radio
            _mediaPlayer.Stop();

            if (_floatingRadio == null)
            {
                _floatingRadio = new FloatingRadio();
            }
            _floatingRadio.Show();
        }

        private void FloatingRadioToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _floatingRadio?.Hide();
        }



        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Clean up NotifyIcon
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                _notifyIcon = null;
            }

            base.OnClosing(e);
            Application.Current.Shutdown();
        }

        private void InitializeSystemTray()
        {
            try
            {
                _notifyIcon = new System.Windows.Forms.NotifyIcon();
                _notifyIcon.Text = "وحي (Wahee) - تطبيق إسلامي";
                
                // Load the new app icon
                try 
                {
                    string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Images", "app_icon.png");
                    if (System.IO.File.Exists(iconPath))
                    {
                        using (var bitmap = new Bitmap(iconPath))
                        {
                            _notifyIcon.Icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon());
                        }
                    }
                    else
                    {
                        _notifyIcon.Icon = SystemIcons.Application;
                    }
                }
                catch 
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                }
                
                // Create context menu
                var contextMenu = new System.Windows.Forms.ContextMenuStrip();
                
                var openItem = new System.Windows.Forms.ToolStripMenuItem("فتح البرنامج");
                openItem.Click += (s, e) => ShowFromTray();
                contextMenu.Items.Add(openItem);
                
                contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
                
                var exitItem = new System.Windows.Forms.ToolStripMenuItem("إنهاء");
                exitItem.Click += (s, e) =>
                {
                    _notifyIcon!.Visible = false;
                    Application.Current.Shutdown();
                };
                contextMenu.Items.Add(exitItem);
                
                _notifyIcon.ContextMenuStrip = contextMenu;
                _notifyIcon.DoubleClick += (s, e) => ShowFromTray();
                _notifyIcon.Visible = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing system tray: {ex.Message}");
            }
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                // Minimize to tray
                Hide();
                if (_notifyIcon != null)
                {
                    _notifyIcon.ShowBalloonTip(2000, "وحي", "البرنامج يعمل في الخلفية", System.Windows.Forms.ToolTipIcon.Info);
                }
            }
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
    }
}
