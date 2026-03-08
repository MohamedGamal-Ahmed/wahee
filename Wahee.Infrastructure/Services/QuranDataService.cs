using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;
using Wahee.Infrastructure.Data;

namespace Wahee.Infrastructure.Services
{
    public class QuranDataService : IQuranDataService
    {
        private readonly HttpClient _httpClient;
        private readonly WaheeDbContext _context;
        private const string RadiosApiUrl = "https://www.mp3quran.net/api/v3/radios?language=ar";

        public QuranDataService(HttpClient httpClient, WaheeDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task<IEnumerable<Radio>> GetRadiosAsync()
        {
            var radios = await _context.Radios.ToListAsync();
            if (!radios.Any())
            {
                await RefreshRadiosAsync();
                radios = await _context.Radios.ToListAsync();
            }
            return radios;
        }

        public async Task RefreshRadiosAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<RadiosResponse>(RadiosApiUrl);
                if (response?.Radios != null)
                {
                    // Clear existing radios and update with new ones
                    _context.Radios.RemoveRange(_context.Radios);
                    await _context.Radios.AddRangeAsync(response.Radios);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log exception in a real app
                Console.WriteLine($"Error fetching radios: {ex.Message}");
            }
        }

        public async Task<Verse?> GetRandomVerseAsync()
        {
            var totalVerses = await _context.Verses.CountAsync();
            if (totalVerses == 0) return null;
            
            var random = new Random();
            var skip = random.Next(totalVerses);
            return await _context.Verses.Include(v => v.Surah).Skip(skip).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Surah>> GetSurahsAsync()
        {
            return await _context.Surahs.OrderBy(s => s.Number).ToListAsync();
        }

        public async Task<Surah?> GetSurahDetailAsync(int number)
        {
            return await _context.Surahs.Include(s => s.Verses).FirstOrDefaultAsync(s => s.Number == number);
        }

        public async Task<IEnumerable<Verse>> SearchQuranAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<Verse>();
            
            var normalizedQuery = NormalizeArabic(query);
            
            // For robust search in SQLite, we fetch verses and filter in memory if they are not too many
            // or we use a simpler approach. 6236 verses is small enough for memory search for now.
            var allVerses = await _context.Verses.Include(v => v.Surah).ToListAsync();
            
            return allVerses
                .Where(v => NormalizeArabic(v.TextAr).Contains(normalizedQuery))
                .Take(50);
        }

        public async Task<Verse?> GetMoodBasedAyahAsync()
        {
            var hour = DateTime.Now.Hour;
            var random = new Random();
            
            var verses = await _context.Verses.Include(v => v.Surah).ToListAsync();
            if (!verses.Any()) return null;

            IEnumerable<Verse> filtered;

            if (hour >= 5 && hour < 12) // Morning: Optimism, Praise
            {
                string[] morningKeywords = { "بشر", "بشري", "الحمد", "سبح", "ضحى", "نهار", "أفلح", "فوز" };
                filtered = verses.Where(v => morningKeywords.Any(k => v.TextAr.Contains(k)));
            }
            else if (hour >= 17 && hour < 22) // Evening: Peace, Mercy
            {
                string[] eveningKeywords = { "رحم", "رحمة", "سكينة", "سلام", "طمأنينة", "رأفة", "مودة" };
                filtered = verses.Where(v => eveningKeywords.Any(k => v.TextAr.Contains(k)));
            }
            else if (hour >= 22 || hour < 5) // Night: Forgiveness, Night Prayer
            {
                string[] nightKeywords = { "ليل", "نجم", "غفر", "مغفرة", "توب", "توبة", "سحر", "هجد" };
                filtered = verses.Where(v => nightKeywords.Any(k => v.TextAr.Contains(k)));
            }
            else // Afternoon/General
            {
                filtered = verses;
            }

            var resultList = filtered.ToList();
            if (!resultList.Any()) return verses[random.Next(verses.Count)];

            return resultList[random.Next(resultList.Count)];
        }

        private string NormalizeArabic(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            // Remove Tashkeel
            string[] tashkeel = { "\u064B", "\u064C", "\u064D", "\u064E", "\u064F", "\u0650", "\u0651", "\u0652" };
            foreach (var t in tashkeel)
                text = text.Replace(t, "");

            // Normalize Alef
            text = text.Replace("\u0622", "\u0627"); // Alef Mad
            text = text.Replace("\u0623", "\u0627"); // Alef Hamza Above
            text = text.Replace("\u0625", "\u0627"); // Alef Hamza Below
            
            // Normalize Yeh
            text = text.Replace("\u0649", "\u064A"); // Alef Maksura to Yeh
            
            // Normalize Teh Marbuta
            text = text.Replace("\u0629", "\u0647"); // Teh Marbuta to Heh

            return text;
        }

        private class RadiosResponse
        {
            public List<Radio>? Radios { get; set; }
        }
    }
}
