using Microsoft.EntityFrameworkCore;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;
using Wahee.Infrastructure.Data;

namespace Wahee.Infrastructure.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly WaheeDbContext _context;
        public SettingsService(WaheeDbContext context) => _context = context;

        public async Task<string?> GetSettingAsync(string key)
        {
            var setting = await _context.UserSettings.FindAsync(key);
            return setting?.Value;
        }

        public async Task SaveSettingAsync(string key, string value)
        {
            var setting = await _context.UserSettings.FindAsync(key);
            if (setting == null)
            {
                _context.UserSettings.Add(new UserSetting { Key = key, Value = value });
            }
            else
            {
                setting.Value = value;
            }
            await _context.SaveChangesAsync();
        }
    }

    public class WidgetSettingsService : IWidgetSettingsService
    {
        private readonly WaheeDbContext _context;
        public WidgetSettingsService(WaheeDbContext context) => _context = context;

        public async Task<WidgetState?> GetWidgetStateAsync(string widgetName)
        {
            return await _context.WidgetStates.FindAsync(widgetName);
        }

        public async Task SaveWidgetStateAsync(WidgetState state)
        {
            var existing = await _context.WidgetStates.FindAsync(state.WidgetName);
            if (existing == null)
            {
                _context.WidgetStates.Add(state);
            }
            else
            {
                existing.IsVisible = state.IsVisible;
                existing.X = state.X;
                existing.Y = state.Y;
            }
            await _context.SaveChangesAsync();
        }
    }
}
