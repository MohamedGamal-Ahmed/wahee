using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;

namespace Wahee.UI.Widgets
{
    public partial class AyahWidget : WaheeWidgetBase
    {
        private Verse? _currentVerse;
        private readonly System.Windows.Media.MediaPlayer _mediaPlayer = new();
        private bool _isPinned;

        public AyahWidget()
        {
            InitializeComponent();
            IsVisibleChanged += Window_IsVisibleChanged;
        }

        protected override void InitializeWidget()
        {
            ApplyPinState();
        }

        protected override async void LoadWidgetData()
        {
            try
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;
                var random = new Random();
                Left = random.Next(80, Math.Max(81, (int)screenWidth - 520));
                Top = random.Next(80, Math.Max(81, (int)screenHeight - 380));

                if (Resources["FadeIn"] is System.Windows.Media.Animation.Storyboard sb)
                {
                    sb.Begin(this);
                }

                await LoadRandomAyahAndTafsirAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Ayah widget: {ex.Message}");
                SetFallbackAyah();
            }
        }

        private async Task LoadRandomAyahAndTafsirAsync()
        {
            try
            {
                LoadingBar.Visibility = Visibility.Visible;
                using var scope = App.ServiceProvider?.CreateScope();
                var quranService = scope?.ServiceProvider.GetService<IQuranDataService>();

                if (quranService == null)
                {
                    SetFallbackAyah("خدمة القرآن غير متاحة الآن.");
                    return;
                }

                _currentVerse = await quranService.GetMoodBasedAyahAsync() ?? await quranService.GetRandomVerseAsync();

                if (_currentVerse == null)
                {
                    SetFallbackAyah("تعذر تحميل آية عشوائية. تأكد من تهيئة بيانات القرآن.");
                    return;
                }

                AyahTextBlock.Text = _currentVerse.TextAr;
                SourceTextBlock.Text = $"سورة {_currentVerse.Surah?.NameAr ?? ""} - آية {_currentVerse.Number}";

                await LoadTafsirForCurrentVerseAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadRandomAyahAndTafsirAsync error: {ex.Message}");
                SetFallbackAyah();
            }
            finally
            {
                LoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private async Task LoadTafsirForCurrentVerseAsync()
        {
            if (_currentVerse == null)
            {
                TafsirTextBlock.Text = "لا يوجد تفسير متاح حالياً.";
                UpdateTafsirHint();
                return;
            }

            try
            {
                using var scope = App.ServiceProvider?.CreateScope();
                var bridge = scope?.ServiceProvider.GetService<IContentBridgeService>();
                if (bridge == null)
                {
                    TafsirTextBlock.Text = "تعذر الوصول لخدمة التفسير.";
                    UpdateTafsirHint();
                    return;
                }

                var surahNumber = _currentVerse.Surah?.Number ?? _currentVerse.SurahId;
                if (surahNumber <= 0)
                {
                    TafsirTextBlock.Text = "تعذر تحديد رقم السورة للتفسير.";
                    UpdateTafsirHint();
                    return;
                }

                var tafsir = await bridge.GetTafsirAsync(surahNumber, _currentVerse.Number);
                TafsirTextBlock.Text = string.IsNullOrWhiteSpace(tafsir) ? "لا يوجد تفسير متاح حالياً." : tafsir;

                AdjustWidgetHeightForContent();
                UpdateTafsirHint();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTafsirForCurrentVerseAsync error: {ex.Message}");
                TafsirTextBlock.Text = "حدث خطأ أثناء تحميل التفسير.";
                UpdateTafsirHint();
            }
        }

        private void AdjustWidgetHeightForContent()
        {
            var ayahLength = AyahTextBlock.Text?.Length ?? 0;
            var tafsirLength = TafsirTextBlock.Text?.Length ?? 0;

            // Dynamic height based on combined content size.
            var desiredHeight = 260 + ((ayahLength + tafsirLength) * 0.32);
            Height = Math.Clamp(desiredHeight, MinHeight, MaxHeight);
        }

        private void UpdateTafsirHint()
        {
            Dispatcher.BeginInvoke(() =>
            {
                UpdateLayout();
                var isClipped = TafsirScrollViewer.ExtentHeight > (TafsirScrollViewer.ViewportHeight + 1);
                TafsirHintText.Visibility = isClipped ? Visibility.Visible : Visibility.Collapsed;
            }, DispatcherPriority.Background);
        }

        private async void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentVerse == null) return;

            try
            {
                LoadingBar.Visibility = Visibility.Visible;
                using var scope = App.ServiceProvider?.CreateScope();
                var bridge = scope?.ServiceProvider.GetService<IContentBridgeService>();
                if (bridge == null) return;

                var surahNumber = _currentVerse.Surah?.Number ?? _currentVerse.SurahId;
                if (surahNumber <= 0) return;

                var audioUrl = await bridge.GetAudioUrlAsync(surahNumber, _currentVerse.Number);
                if (!string.IsNullOrWhiteSpace(audioUrl))
                {
                    _mediaPlayer.Stop();
                    _mediaPlayer.Open(new Uri(audioUrl));
                    _mediaPlayer.Play();
                }
            }
            finally
            {
                LoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            await LoadRandomAyahAndTafsirAsync();
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
            PinButton.Background = _isPinned
                ? (System.Windows.Media.Brush)FindResource("AccentBrush")
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x18, 0xFF, 0xFF, 0xFF));
            PinButton.Foreground = _isPinned
                ? System.Windows.Media.Brushes.White
                : (System.Windows.Media.Brush)FindResource("TextForegroundBrush");
        }

        private void SetFallbackAyah(string? errorMessage = null)
        {
            AyahTextBlock.Text = "إِنَّ مَعَ الْعُسْرِ يُسْرًا";
            SourceTextBlock.Text = "سورة الشرح - آية 6";
            TafsirTextBlock.Text = errorMessage ?? "تفسير الآية غير متاح حالياً، حاول مرة أخرى.";
            AdjustWidgetHeightForContent();
            UpdateTafsirHint();
        }

        private void Window_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                _mediaPlayer.Stop();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
            Hide();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _mediaPlayer.Stop();
            e.Cancel = true;
            Hide();
        }
    }
}

