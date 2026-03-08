// Task Summary:
// - Phase 2 / Task 1: Added QuranViewModel with search/surah loading commands.
using System.Collections.ObjectModel;
using System.Windows.Input;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;
using Wahee.Core.ViewModels;

namespace Wahee.UI.ViewModels
{
    public class QuranViewModel : BaseViewModel
    {
        private readonly IQuranDataService _quranDataService;

        private string _searchQuery = string.Empty;
        public string SearchQuery { get => _searchQuery; set => SetProperty(ref _searchQuery, value); }

        private ObservableCollection<Verse> _searchResults = new();
        public ObservableCollection<Verse> SearchResults { get => _searchResults; set => SetProperty(ref _searchResults, value); }

        private ObservableCollection<Surah> _surahs = new();
        public ObservableCollection<Surah> Surahs { get => _surahs; set => SetProperty(ref _surahs, value); }

        private Surah? _selectedSurah;
        public Surah? SelectedSurah
        {
            get => _selectedSurah;
            set
            {
                if (SetProperty(ref _selectedSurah, value) && value != null)
                {
                    _ = LoadSurahDetailAsync(value.Number);
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        private string _statusMessage = "اختر سورة للبدء";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ICommand SearchCommand { get; }
        public ICommand LoadSurahsCommand { get; }

        public QuranViewModel(IQuranDataService quranDataService)
        {
            _quranDataService = quranDataService;
            SearchCommand = new AsyncRelayCommand(_ => SearchAsync());
            LoadSurahsCommand = new AsyncRelayCommand(_ => LoadSurahsAsync());
        }

        public async Task LoadSurahsAsync()
        {
            try
            {
                IsLoading = true;
                var surahs = await _quranDataService.GetSurahsAsync();
                Surahs = new ObservableCollection<Surah>(surahs);
            }
            catch (Exception ex)
            {
                StatusMessage = "تعذر تحميل السور";
                System.Diagnostics.Debug.WriteLine($"[QuranViewModel] {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                StatusMessage = "اكتب نص البحث أولاً";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = string.Empty;
                var results = await _quranDataService.SearchQuranAsync(SearchQuery);
                SearchResults = new ObservableCollection<Verse>(results);
                StatusMessage = SearchResults.Count == 0 ? "لا توجد نتائج" : $"{SearchResults.Count} نتيجة";
            }
            catch (Exception ex)
            {
                StatusMessage = "حدث خطأ أثناء البحث";
                System.Diagnostics.Debug.WriteLine($"[QuranViewModel] Search error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadSurahDetailAsync(int surahNumber)
        {
            try
            {
                IsLoading = true;
                var detail = await _quranDataService.GetSurahDetailAsync(surahNumber);
                if (detail != null)
                {
                    SearchResults = new ObservableCollection<Verse>(detail.Verses);
                    StatusMessage = $"سورة {detail.NameAr}";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[QuranViewModel] Surah detail error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
