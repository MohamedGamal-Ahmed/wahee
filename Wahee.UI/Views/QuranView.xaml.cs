using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;

namespace Wahee.UI.Views
{
    public partial class QuranView : UserControl
    {
        private readonly IQuranDataService? _quranService;

        public QuranView()
        {
            InitializeComponent();
            _quranService = App.ServiceProvider?.GetService<IQuranDataService>();
            SearchBox.Text = SearchBox.Tag?.ToString();
            LoadSurahs();
        }

        private async void LoadSurahs()
        {
            if (_quranService == null) return;
            LoadingBar.Visibility = Visibility.Visible;
            var surahs = await _quranService.GetSurahsAsync();
            SurahListBox.ItemsSource = surahs;
            LoadingBar.Visibility = Visibility.Collapsed;
        }

        private async void SurahListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SurahListBox.SelectedItem is Surah surah && _quranService != null)
            {
                LoadingBar.Visibility = Visibility.Visible;
                SelectedSurahTxt.Text = $"سورة {surah.NameAr}";
                var details = await _quranService.GetSurahDetailAsync(surah.Number);
                VersesListControl.ItemsSource = details?.Verses;
                VersesScrollViewer.ScrollToHome();
                LoadingBar.Visibility = Visibility.Collapsed;
            }
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchBox.Text;
            if (string.IsNullOrWhiteSpace(query) || query == SearchBox.Tag?.ToString()) return;

            if (_quranService != null)
            {
                LoadingBar.Visibility = Visibility.Visible;
                SelectedSurahTxt.Text = $"نتائج البحث عن: {query}";
                var results = await _quranService.SearchQuranAsync(query);
                VersesListControl.ItemsSource = results;
                LoadingBar.Visibility = Visibility.Collapsed;
                
                if (!results.Any())
                {
                    MessageBox.Show("لم يتم العثور على نتائج.", "نتائج البحث", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private async void Tafsir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Verse verse)
            {
                try 
                {
                    LoadingBar.Visibility = Visibility.Visible;
                    using (var scope = App.ServiceProvider?.CreateScope())
                    {
                        var bridge = scope?.ServiceProvider.GetService<IContentBridgeService>();
                        if (bridge != null)
                        {
                            var tafsir = await bridge.GetTafsirAsync(verse.SurahId, verse.Number);
                            
                            // Using a MessageBox for now as requested for simplicity
                            MessageBox.Show(tafsir, $"تفسير الآية {verse.Number} - سورة {verse.Surah?.NameAr}", 
                                MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("تعذر جلب التفسير، يرجى التحقق من الاتصال بالإنترنت.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == SearchBox.Tag?.ToString())
            {
                SearchBox.Text = "";
                SearchBox.Opacity = 1.0;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = SearchBox.Tag?.ToString();
                SearchBox.Opacity = 0.5;
            }
        }

        private void SearchBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Search_Click(sender, e);
            }
        }

        // Audio playback
        private readonly System.Windows.Media.MediaPlayer _audioPlayer = new();
        private Verse? _currentPlayingVerse;

        private void PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Verse verse)
            {
                try
                {
                    LoadingBar.Visibility = Visibility.Visible;
                    
                    // Build audio URL - using Al-Afasy recitation from MP3Quran
                    // Format: https://server8.mp3quran.net/afs/XXX.mp3 where XXX is surah number
                    string surahNum = verse.SurahId.ToString("D3");
                    string audioUrl = $"https://server8.mp3quran.net/afs/{surahNum}.mp3";
                    
                    _currentPlayingVerse = verse;
                    _audioPlayer.Open(new Uri(audioUrl));
                    _audioPlayer.Play();
                    
                    // Show player bar
                    NowPlayingTxt.Text = $"سورة {verse.Surah?.NameAr ?? ""}";
                    NowPlayingSurah.Text = $"الشيخ مشاري العفاسي";
                    AudioPlayerBar.Visibility = Visibility.Visible;
                    
                    _audioPlayer.MediaOpened += (s, args) => 
                    {
                        Dispatcher.Invoke(() => LoadingBar.Visibility = Visibility.Collapsed);
                    };
                    
                    _audioPlayer.MediaFailed += (s, args) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LoadingBar.Visibility = Visibility.Collapsed;
                            AudioPlayerBar.Visibility = Visibility.Collapsed;
                            MessageBox.Show("تعذر تحميل الملف الصوتي. تحقق من اتصالك بالإنترنت.", 
                                "خطأ في التشغيل", MessageBoxButton.OK, MessageBoxImage.Warning);
                        });
                    };

                    _audioPlayer.MediaEnded += (s, args) =>
                    {
                        Dispatcher.Invoke(() => AudioPlayerBar.Visibility = Visibility.Collapsed);
                    };
                }
                catch (Exception ex)
                {
                    LoadingBar.Visibility = Visibility.Collapsed;
                    MessageBox.Show($"خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void StopAudio_Click(object sender, RoutedEventArgs e)
        {
            StopAudio();
        }

        public void StopAudio()
        {
            _audioPlayer.Stop();
            _currentPlayingVerse = null;
            AudioPlayerBar.Visibility = Visibility.Collapsed;
        }
    }
}
