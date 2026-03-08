using System.IO;
using System.Text.Json;
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
        private List<MushafPage> _pages = new();
        private int _currentLeftPageIndex = 1; // Even page (left in RTL)
        private const int TotalPages = 604;
        private readonly string _imagesBasePath;

        public MushafView()
        {
            InitializeComponent();
            
            _quranDataService = App.ServiceProvider?.GetService<IQuranDataService>();
            
            // Set images base path relative to EXE
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Try standard locations
            string[] possiblePaths = new[] 
            {
                Path.Combine(baseDir, "Quran-Data-version-2.0", "data", "quran_image"),
                Path.Combine(baseDir, "Data", "quran_image"),
                Path.Combine(baseDir, "..", "..", "..", "Quran-Data-version-2.0", "data", "quran_image") // Dev fallback
            };

            _imagesBasePath = possiblePaths.FirstOrDefault(Directory.Exists) ?? possiblePaths[0];
            
            Loaded += MushafView_Loaded;
        }

        private async void MushafView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPagesDataAsync();
            await LoadSurahsAsync();
            NavigateToPage(1);
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
            
            var surahs = await _quranDataService.GetSurahsAsync();
            SurahComboBox.ItemsSource = surahs;
        }

        private void NavigateToPage(int pageNumber)
        {
            // Ensure we're on an odd page (right page in RTL layout)
            if (pageNumber % 2 == 0) pageNumber--;
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > TotalPages) pageNumber = TotalPages - 1;
            
            _currentLeftPageIndex = pageNumber + 1;
            int rightPage = pageNumber;
            int leftPage = pageNumber + 1;
            
            // Load right page image (odd page)
            LoadPageImage(RightPageImage, rightPage);
            
            // Load left page image (even page)
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
            var leftPageData = _pages.FirstOrDefault(p => p.Page == leftPage);
            
            string surahInfo = "";
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
            NextBtn.IsEnabled = currentPage < TotalPages - 1;
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            int newPage = _currentLeftPageIndex - 3; // Go back 2 pages (to previous spread)
            NavigateToPage(newPage);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            int newPage = _currentLeftPageIndex + 1; // Go forward 2 pages
            NavigateToPage(newPage);
        }

        private void GoToPage_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PageNumberBox.Text, out int page))
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
            if (SurahComboBox.SelectedItem is Surah surah)
            {
                // Find the page where this surah starts
                var pageData = _pages.FirstOrDefault(p => 
                    p.Start?.SurahNumber == surah.Number);
                    
                if (pageData != null)
                {
                    NavigateToPage(pageData.Page);
                }
            }
        }
    }

    // Helper classes for JSON deserialization
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
