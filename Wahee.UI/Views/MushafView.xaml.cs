using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;

namespace Wahee.UI.Views
{
    public partial class MushafView : UserControl
    {
        private readonly IQuranDataService? _quranDataService;
        private readonly ISettingsService? _settingsService;
        private List<MushafPage> _pages = new();
        private int _currentLeftPageIndex = 1;
        private const int TotalPages = 604;
        private readonly string _imagesBasePath;
        private const string LastMushafPageSettingKey = "LastMushafPage";
        private bool _isInitializing;
        private bool _isUpdatingSurahSelection;
        private int _lastSavedPage = 1;
        private int _currentRightPage = 1;

        public MushafView()
        {
            InitializeComponent();

            _quranDataService = App.ServiceProvider?.GetService<IQuranDataService>();
            _settingsService = App.ServiceProvider?.GetService<ISettingsService>();

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var possiblePaths = new[]
            {
                Path.Combine(baseDir, "Quran-Data-version-2.0", "data", "quran_image"),
                Path.Combine(baseDir, "Data", "quran_image"),
                Path.Combine(baseDir, "..", "..", "..", "Quran-Data-version-2.0", "data", "quran_image")
            };

            _imagesBasePath = possiblePaths.FirstOrDefault(Directory.Exists) ?? possiblePaths[0];
            Loaded += MushafView_Loaded;
            Unloaded += MushafView_Unloaded;
        }

        private async void MushafView_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitializing = true;

            await LoadPagesDataAsync();
            await LoadSurahsAsync();
            await RestoreLastReadingPositionAsync();

            _isInitializing = false;
        }

        private async Task RestoreLastReadingPositionAsync()
        {
            var pageToOpen = 1;

            try
            {
                if (_settingsService != null)
                {
                    var savedPageRaw = await _settingsService.GetSettingAsync(LastMushafPageSettingKey);
                    if (int.TryParse(savedPageRaw, out var savedPage) && savedPage >= 1 && savedPage <= TotalPages)
                    {
                        pageToOpen = savedPage;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring last mushaf page: {ex.Message}");
            }

            _lastSavedPage = pageToOpen;
            UpdateContinueReadingButton();
            NavigateToPage(pageToOpen);
        }

        private async Task LoadPagesDataAsync()
        {
            try
            {
                var pagesJsonPath = Path.Combine(Path.GetDirectoryName(_imagesBasePath)!, "pagesQuran.json");
                if (File.Exists(pagesJsonPath))
                {
                    var json = await File.ReadAllTextAsync(pagesJsonPath);
                    _pages = JsonSerializer.Deserialize<List<MushafPage>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<MushafPage>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading pages data: {ex.Message}");
            }
        }

        private async Task LoadSurahsAsync()
        {
            if (_quranDataService == null) return;

            var surahs = (await _quranDataService.GetSurahsAsync()).OrderBy(s => s.Number).ToList();
            SurahComboBox.ItemsSource = surahs;
        }

        private void NavigateToPage(int pageNumber)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > TotalPages) pageNumber = TotalPages;

            var rightPage = pageNumber;
            var leftPage = pageNumber + 1;
            _currentLeftPageIndex = leftPage;
            _currentRightPage = rightPage;

            LoadPageImage(RightPageImage, rightPage);

            if (leftPage <= TotalPages)
            {
                LoadPageImage(LeftPageImage, leftPage);
                LeftPageImage.Visibility = Visibility.Visible;
            }
            else
            {
                LeftPageImage.Visibility = Visibility.Collapsed;
            }

            UpdatePageInfo(rightPage, leftPage);
            UpdateNavigationButtons(rightPage);
            UpdateSelectedSurahForPage(rightPage);
            _ = SaveLastReadingPositionAsync(rightPage);
        }

        private async Task SaveLastReadingPositionAsync(int pageNumber)
        {
            _lastSavedPage = pageNumber;
            UpdateContinueReadingButton();

            try
            {
                if (_settingsService != null)
                {
                    await _settingsService.SaveSettingAsync(LastMushafPageSettingKey, pageNumber.ToString());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving last mushaf page: {ex.Message}");
            }
        }

        private void UpdateContinueReadingButton()
        {
            ContinueReadingBtn.Content = _lastSavedPage > 1
                ? $"متابعة القراءة (صفحة {_lastSavedPage})"
                : "متابعة القراءة";
        }

        private void ContinueReading_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(_lastSavedPage > 0 ? _lastSavedPage : 1);
        }

        private async void MushafView_Unloaded(object sender, RoutedEventArgs e)
        {
            await SaveLastReadingPositionAsync(_currentRightPage > 0 ? _currentRightPage : 1);
        }

        private void UpdateSelectedSurahForPage(int pageNumber)
        {
            if (SurahComboBox.ItemsSource is not IEnumerable<Surah> surahs)
            {
                return;
            }

            var pageData = _pages.FirstOrDefault(p => p.Page == pageNumber);
            var surahNumber = pageData?.Start?.SurahNumber ?? pageData?.End?.SurahNumber ?? 0;
            if (surahNumber <= 0)
            {
                return;
            }

            var selectedSurah = surahs.FirstOrDefault(s => s.Number == surahNumber);
            if (selectedSurah == null)
            {
                return;
            }

            if (SurahComboBox.SelectedItem is Surah current && current.Number == selectedSurah.Number)
            {
                return;
            }

            try
            {
                _isUpdatingSurahSelection = true;
                SurahComboBox.SelectedItem = selectedSurah;
            }
            finally
            {
                _isUpdatingSurahSelection = false;
            }
        }

        private int FindSurahStartPage(int surahNumber)
        {
            if (_pages.Count == 0)
            {
                return 1;
            }

            var exactStart = _pages.FirstOrDefault(p => p.Start?.SurahNumber == surahNumber);
            if (exactStart != null)
            {
                return exactStart.Page;
            }

            var withinRange = _pages.FirstOrDefault(p =>
                p.Start != null && p.End != null &&
                p.Start.SurahNumber <= surahNumber && p.End.SurahNumber >= surahNumber);
            if (withinRange != null)
            {
                return withinRange.Page;
            }

            var nearest = _pages
                .Where(p => p.Start != null && p.Start.SurahNumber >= surahNumber)
                .OrderBy(p => p.Start!.SurahNumber)
                .ThenBy(p => p.Page)
                .FirstOrDefault();

            return nearest?.Page ?? 1;
        }

        private void LoadPageImage(System.Windows.Controls.Image imageControl, int pageNumber)
        {
            try
            {
                var imagePath = Path.Combine(_imagesBasePath, $"{pageNumber}.png");
                if (File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imageControl.Source = bitmap;
                }
                else
                {
                    imageControl.Source = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading page image: {ex.Message}");
                imageControl.Source = null;
            }
        }

        private void UpdatePageInfo(int rightPage, int leftPage)
        {
            var rightPageData = _pages.FirstOrDefault(p => p.Page == rightPage);

            var surahInfo = "";
            if (rightPageData?.Start?.Name?.Ar != null)
            {
                surahInfo = $"سورة {rightPageData.Start.Name.Ar}";
            }

            CurrentPageTxt.Text = leftPage <= TotalPages
                ? $"{surahInfo} - صفحة {rightPage} و {leftPage}"
                : $"{surahInfo} - صفحة {rightPage}";

            PageCounterTxt.Text = $"{rightPage}-{Math.Min(leftPage, TotalPages)} / {TotalPages}";
            PageNumberBox.Text = rightPage.ToString();
        }

        private void UpdateNavigationButtons(int currentPage)
        {
            PrevBtn.IsEnabled = currentPage > 1;
            NextBtn.IsEnabled = currentPage < TotalPages;
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            var newPage = _currentRightPage - 2;
            NavigateToPage(newPage);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var newPage = _currentRightPage + 2;
            NavigateToPage(newPage);
        }

        private void GoToPage_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PageNumberBox.Text, out var page))
            {
                NavigateToPage(page);
            }
        }

        private void PageNumberBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                GoToPage_Click(sender, e);
            }
        }

        private void SurahComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing || _isUpdatingSurahSelection)
            {
                return;
            }

            if (SurahComboBox.SelectedItem is Surah surah)
            {
                var page = FindSurahStartPage(surah.Number);
                NavigateToPage(page);
            }
        }
    }

    public class MushafPage
    {
        public int Page { get; set; }
        public MushafPageImage? Image { get; set; }
        public MushafPageVerse? Start { get; set; }
        public MushafPageVerse? End { get; set; }
    }

    public class MushafPageImage
    {
        public string Url { get; set; } = "";
    }

    public class MushafPageVerse
    {
        [JsonPropertyName("surah_number")]
        public int SurahNumber { get; set; }

        public int Verse { get; set; }
        public MushafSurahName? Name { get; set; }
    }

    public class MushafSurahName
    {
        public string Ar { get; set; } = "";
        public string En { get; set; } = "";
        public string Transliteration { get; set; } = "";
    }
}

