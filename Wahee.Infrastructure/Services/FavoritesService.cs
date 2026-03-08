using System.Text.Json;
using Wahee.Core.Interfaces;

namespace Wahee.Infrastructure.Services
{
    public class FavoritesService : IFavoritesService
    {
        private readonly ISettingsService _settingsService;
        private const string FavoriteRadiosKey = "favorite_radios";
        private HashSet<int> _cachedFavorites = new();
        private bool _isLoaded = false;

        public FavoritesService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private async Task EnsureLoadedAsync()
        {
            if (_isLoaded) return;
            
            var json = await _settingsService.GetSettingAsync(FavoriteRadiosKey);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var ids = JsonSerializer.Deserialize<List<int>>(json);
                    _cachedFavorites = ids != null ? new HashSet<int>(ids) : new HashSet<int>();
                }
                catch
                {
                    _cachedFavorites = new HashSet<int>();
                }
            }
            _isLoaded = true;
        }

        private async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_cachedFavorites.ToList());
            await _settingsService.SaveSettingAsync(FavoriteRadiosKey, json);
        }

        public async Task<IEnumerable<int>> GetFavoriteRadioIdsAsync()
        {
            await EnsureLoadedAsync();
            return _cachedFavorites.ToList();
        }

        public async Task AddFavoriteRadioAsync(int radioId)
        {
            await EnsureLoadedAsync();
            if (_cachedFavorites.Add(radioId))
            {
                await SaveAsync();
            }
        }

        public async Task RemoveFavoriteRadioAsync(int radioId)
        {
            await EnsureLoadedAsync();
            if (_cachedFavorites.Remove(radioId))
            {
                await SaveAsync();
            }
        }

        public async Task<bool> IsFavoriteRadioAsync(int radioId)
        {
            await EnsureLoadedAsync();
            return _cachedFavorites.Contains(radioId);
        }

        public async Task ToggleFavoriteRadioAsync(int radioId)
        {
            await EnsureLoadedAsync();
            if (_cachedFavorites.Contains(radioId))
            {
                _cachedFavorites.Remove(radioId);
            }
            else
            {
                _cachedFavorites.Add(radioId);
            }
            await SaveAsync();
        }
    }
}
