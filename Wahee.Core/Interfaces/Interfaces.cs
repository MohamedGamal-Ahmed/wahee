using Wahee.Core.Models;

namespace Wahee.Core.Interfaces
{
    public interface IInspirationRepository
    {
        Task<DailyInspiration?> GetRandomInspirationAsync(string type);
        Task<IEnumerable<DailyInspiration>> GetAllAsync();
        Task AddAsync(DailyInspiration inspiration);
    }

    public interface IWallpaperService
    {
        void SetWallpaper(string imagePath);
        Task ChangeWallpaperAsync(string category);
        Task CycleWallpaperAsync(string folderPath, CancellationToken ct);
    }

    public interface ISettingsService
    {
        Task<string?> GetSettingAsync(string key);
        Task SaveSettingAsync(string key, string value);
    }

    public interface IWidgetSettingsService
    {
        Task<WidgetState?> GetWidgetStateAsync(string widgetName);
        Task SaveWidgetStateAsync(WidgetState state);
    }

    public interface IQuranDataService
    {
        Task<IEnumerable<Radio>> GetRadiosAsync();
        Task RefreshRadiosAsync();
        Task<Verse?> GetRandomVerseAsync();
        Task<IEnumerable<Surah>> GetSurahsAsync();
        Task<Surah?> GetSurahDetailAsync(int number);
        Task<IEnumerable<Verse>> SearchQuranAsync(string query);
        Task<Verse?> GetMoodBasedAyahAsync();
    }

    public interface IContentBridgeService
    {
        Task<string> GetTafsirAsync(int surahNumber, int verseNumber);
        Task<string> GetAudioUrlAsync(int surahNumber, int verseNumber);
    }
}
