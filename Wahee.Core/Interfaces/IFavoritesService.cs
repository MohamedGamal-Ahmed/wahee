namespace Wahee.Core.Interfaces
{
    public interface IFavoritesService
    {
        Task<IEnumerable<int>> GetFavoriteRadioIdsAsync();
        Task AddFavoriteRadioAsync(int radioId);
        Task RemoveFavoriteRadioAsync(int radioId);
        Task<bool> IsFavoriteRadioAsync(int radioId);
        Task ToggleFavoriteRadioAsync(int radioId);
    }
}
