using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;

namespace Wahee.UI.Widgets
{
    public partial class AyahWidget : WaheeWidgetBase
    {
        public AyahWidget()
        {
            InitializeComponent();
        }

        protected override void InitializeWidget()
        {
            // Set initial position if saved in DB/Settings
        }

        private Verse? _currentVerse;
        private readonly System.Windows.Media.MediaPlayer _mediaPlayer = new();

        protected override async void LoadWidgetData()
        {
            try
            {
                // Random position logic
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;
                var random = new Random();
                this.Left = random.Next(100, Math.Max(101, (int)screenWidth - 500));
                this.Top = random.Next(100, Math.Max(101, (int)screenHeight - 300));

                // Start FadeIn Animation
                if (Resources["FadeIn"] is System.Windows.Media.Animation.Storyboard sb)
                {
                    sb.Begin(this);
                }

                using (var scope = App.ServiceProvider?.CreateScope())
                {
                    var quranService = scope?.ServiceProvider.GetService<IQuranDataService>();
                    if (quranService != null)
                    {
                        var verse = await quranService.GetMoodBasedAyahAsync();
                        if (verse != null)
                        {
                            _currentVerse = verse;
                            AyahTextBlock.Text = verse.TextAr;
                            SourceTextBlock.Text = $"سورة {verse.Surah?.NameAr} - آية {verse.Number}";
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading Ayah: {ex.Message}");
                AyahTextBlock.Text = "إِنَّ مَعَ الْعُسْرِ يُسْرًا";
                SourceTextBlock.Text = "سورة الشرح - آية 6";
            }
        }

        private async void TafsirBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentVerse == null) return;
            
            try 
            {
                LoadingBar.Visibility = Visibility.Visible;
                using (var scope = App.ServiceProvider?.CreateScope())
                {
                    var bridge = scope?.ServiceProvider.GetService<IContentBridgeService>();
                    if (bridge != null)
                    {
                        var tafsir = await bridge.GetTafsirAsync(_currentVerse.SurahId, _currentVerse.Number);
                        TafsirTextBlock.Text = tafsir;
                        TafsirPanel.Visibility = Visibility.Visible;
                    }
                }
            }
            finally 
            {
                LoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseTafsir_Click(object sender, RoutedEventArgs e)
        {
            TafsirPanel.Visibility = Visibility.Collapsed;
        }

        private async void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentVerse == null) return;

            try 
            {
                LoadingBar.Visibility = Visibility.Visible;
                using (var scope = App.ServiceProvider?.CreateScope())
                {
                    var bridge = scope?.ServiceProvider.GetService<IContentBridgeService>();
                    if (bridge != null)
                    {
                        var audioUrl = await bridge.GetAudioUrlAsync(_currentVerse.SurahId, _currentVerse.Number);
                        if (!string.IsNullOrEmpty(audioUrl))
                        {
                            _mediaPlayer.Stop();
                            _mediaPlayer.Open(new Uri(audioUrl));
                            _mediaPlayer.Play();
                        }
                    }
                }
            }
            finally 
            {
                LoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private void TadabburBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("قريباً: مقاطع تدبر وبث مباشر لهذه الآية! ✨", "وحي - التدبر", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Stop();
            this.Hide();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
