using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;
using Wahee.Infrastructure.Data;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Threading;

namespace Wahee.Infrastructure.Services
{
    public class InspirationRepository : IInspirationRepository
    {
        private readonly WaheeDbContext _context;
        public InspirationRepository(WaheeDbContext context) => _context = context;

        public async Task<DailyInspiration?> GetRandomInspirationAsync(string type)
        {
            return await _context.DailyInspirations
                .Where(i => i.Type == type)
                .OrderBy(r => Guid.NewGuid())
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DailyInspiration>> GetAllAsync() => await _context.DailyInspirations.ToListAsync();
        public async Task AddAsync(DailyInspiration inspiration)
        {
            _context.DailyInspirations.Add(inspiration);
            await _context.SaveChangesAsync();
        }
    }

    public class WallpaperService : IWallpaperService
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        private readonly HttpClient _httpClient;

        public WallpaperService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private static readonly Dictionary<string, string[]> Categories = new()
        {
            ["Nature"] = new[] {
                "https://images.unsplash.com/photo-1470770841072-f978cf4d019e?q=80&w=1920",
                "https://images.unsplash.com/photo-1464822759023-fed622ff2c3b?q=80&w=1920",
                "https://images.unsplash.com/photo-1441974231531-c6227db76b6e?q=80&w=1920"
            },
            ["Architecture"] = new[] {
                "https://images.unsplash.com/photo-1548013146-72479768bbaa?q=80&w=1920",
                "https://images.unsplash.com/photo-1564507592333-c60657eaa0ae?q=80&w=1920",
                "https://images.unsplash.com/photo-1590822124571-06900ee7418a?q=80&w=1920"
            },
            ["Minimalist"] = new[] {
                "https://images.unsplash.com/photo-1494438639946-1ebd1d20bf85?q=80&w=1920",
                "https://images.unsplash.com/photo-1550684848-fac1c5b4e853?q=80&w=1920",
                "https://images.unsplash.com/photo-1499195333224-3ce974eecfb4?q=80&w=1920"
            }
        };

        public void SetWallpaper(string imagePath)
        {
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, imagePath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        public async Task ChangeWallpaperAsync(string category)
        {
            try
            {
                string? localImagePath = null;
                
                // Try local images first if they exist
                string localFolder = @"D:\MO-Gamal\Projects\Wahee\Quran-Data-version-2.0\quran_image";
                if (Directory.Exists(localFolder))
                {
                    var localFiles = Directory.GetFiles(localFolder, "*.*")
                        .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png"))
                        .ToList();
                    
                    if (localFiles.Count > 0 && new Random().Next(2) == 0) // 50% chance to use local
                    {
                        localImagePath = localFiles[new Random().Next(localFiles.Count)];
                    }
                }

                if (localImagePath != null)
                {
                    SetWallpaper(localImagePath);
                    return;
                }

                // Fallback to Unsplash
                if (!Categories.TryGetValue(category, out var urls))
                {
                    urls = Categories["Nature"];
                }

                var random = new Random();
                var url = urls[random.Next(urls.Length)];

                var tempPath = Path.Combine(Path.GetTempPath(), "wahee_wallpaper.jpg");
                var bytes = await _httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(tempPath, bytes);

                SetWallpaper(tempPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Wallpaper Error: {ex.Message}");
            }
        }

        public async Task CycleWallpaperAsync(string folderPath, CancellationToken ct)
        {
            if (!Directory.Exists(folderPath)) return;

            while (!ct.IsCancellationRequested)
            {
                var files = Directory.GetFiles(folderPath, "*.*")
                    .Where(s => s.EndsWith(".jpg") || s.EndsWith(".png") || s.EndsWith(".bmp"))
                    .ToList();

                if (files.Count > 0)
                {
                    var random = new Random();
                    var nextFile = files[random.Next(files.Count)];
                    SetWallpaper(nextFile);
                }

                try { await Task.Delay(TimeSpan.FromMinutes(10), ct); }
                catch (TaskCanceledException) { break; }
            }
        }
    }
}
