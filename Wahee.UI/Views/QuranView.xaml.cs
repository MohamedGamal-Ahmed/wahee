// Task Summary:
// - Phase 2 / Task 3: Wired QuranView to QuranViewModel and minimized code-behind to UI-only behaviors.
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;
using Wahee.UI.ViewModels;

namespace Wahee.UI.Views
{
    public partial class QuranView : UserControl
    {
        private readonly QuranViewModel _viewModel;
        private readonly System.Windows.Media.MediaPlayer _audioPlayer = new();
        private Verse? _currentPlayingVerse;

        public QuranView()
        {
            InitializeComponent();

            _viewModel = App.ServiceProvider!.GetRequiredService<QuranViewModel>();
            DataContext = _viewModel;

            Loaded += async (s, e) => await _viewModel.LoadSurahsAsync();
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(QuranViewModel.SelectedSurah) || e.PropertyName == nameof(QuranViewModel.SearchResults))
            {
                VersesScrollViewer.ScrollToHome();
            }
        }

        private void SearchBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && _viewModel.SearchCommand.CanExecute(null))
            {
                _viewModel.SearchCommand.Execute(null);
            }
        }

        private async void Tafsir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Verse verse)
            {
                try
                {
                    using (var scope = App.ServiceProvider?.CreateScope())
                    {
                        var bridge = scope?.ServiceProvider.GetService<IContentBridgeService>();
                        if (bridge != null)
                        {
                            var tafsir = await bridge.GetTafsirAsync(verse.SurahId, verse.Number);
                            MessageBox.Show(
                                tafsir,
                                $"تفسير الآية {verse.Number} - سورة {verse.Surah?.NameAr}",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information,
                                MessageBoxResult.OK,
                                MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("تعذر جلب التفسير، يرجى التحقق من الاتصال بالإنترنت.", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Verse verse)
            {
                try
                {
                    string surahNum = verse.SurahId.ToString("D3");
                    string audioUrl = $"https://server8.mp3quran.net/afs/{surahNum}.mp3";

                    _currentPlayingVerse = verse;
                    _audioPlayer.Open(new Uri(audioUrl));
                    _audioPlayer.Play();

                    NowPlayingTxt.Text = $"سورة {verse.Surah?.NameAr ?? string.Empty}";
                    NowPlayingSurah.Text = "الشيخ مشاري العفاسي";
                    AudioPlayerBar.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
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
