// Task Summary:
// - Phase 2 / Task 1: Added AzkarViewModel with command-based dhikr flow.
using System.Collections.ObjectModel;
using System.Windows.Input;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;
using Wahee.Core.ViewModels;

namespace Wahee.UI.ViewModels
{
    public class AzkarViewModel : BaseViewModel
    {
        private readonly IInspirationRepository _inspirationRepository;

        private ObservableCollection<DailyInspiration> _azkarList = new();
        public ObservableCollection<DailyInspiration> AzkarList { get => _azkarList; set => SetProperty(ref _azkarList, value); }

        private DailyInspiration? _currentAzkar;
        public DailyInspiration? CurrentAzkar { get => _currentAzkar; set => SetProperty(ref _currentAzkar, value); }

        private int _currentCount;
        public int CurrentCount { get => _currentCount; set => SetProperty(ref _currentCount, value); }

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }

        public ICommand NextAzkarCommand { get; }
        public ICommand CountCommand { get; }
        public ICommand LoadAzkarCommand { get; }

        public AzkarViewModel(IInspirationRepository inspirationRepository)
        {
            _inspirationRepository = inspirationRepository;
            NextAzkarCommand = new AsyncRelayCommand(_ => LoadNextAzkarAsync());
            CountCommand = new RelayCommand(_ => CurrentCount++);
            LoadAzkarCommand = new AsyncRelayCommand(_ => LoadAllAzkarAsync());
        }

        public async Task LoadAllAzkarAsync()
        {
            try
            {
                IsLoading = true;
                var all = await _inspirationRepository.GetAllAsync();
                AzkarList = new ObservableCollection<DailyInspiration>(all.Where(x => x.Type == "Dhikr"));
                CurrentAzkar = AzkarList.FirstOrDefault();
                CurrentCount = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AzkarViewModel] {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadNextAzkarAsync()
        {
            var next = await _inspirationRepository.GetRandomInspirationAsync("Dhikr");
            CurrentAzkar = next;
            CurrentCount = 0;
        }
    }
}
