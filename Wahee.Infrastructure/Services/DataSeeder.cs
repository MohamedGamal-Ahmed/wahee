using System.IO;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wahee.Core.Models;
using Wahee.Infrastructure.Data;

namespace Wahee.Infrastructure.Services
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(WaheeDbContext context)
        {
            // Seed Inspirations if empty
            if (!context.DailyInspirations.Any())
            {
                var inspirations = GetDefaultInspirations();
                await context.DailyInspirations.AddRangeAsync(inspirations);
                await context.SaveChangesAsync();
            }

            // Seed Quran Data if empty
            if (!context.Surahs.Any())
            {
                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.json");
                // If not in base directory, try the absolute path provided by user
                if (!File.Exists(jsonPath))
                {
                    jsonPath = @"D:\MO-Gamal\Projects\Wahee\database.json";
                }

                if (File.Exists(jsonPath))
                {
                    using FileStream openStream = File.OpenRead(jsonPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var jsonSurahs = await JsonSerializer.DeserializeAsync<List<JsonSurah>>(openStream, options);

                    if (jsonSurahs != null)
                    {
                        var surahs = new List<Surah>();
                        foreach (var jSurah in jsonSurahs)
                        {
                            var surah = new Surah
                            {
                                Number = jSurah.Number,
                                NameAr = jSurah.Name?.Ar ?? string.Empty,
                                NameEn = jSurah.Name?.En ?? string.Empty,
                                Transliteration = jSurah.Name?.Transliteration ?? string.Empty,
                                VersesCount = jSurah.Verses_Count,
                                Verses = jSurah.Verses?.Select(v => new Verse
                                {
                                    Number = v.Number,
                                    TextAr = v.Text?.Ar ?? string.Empty,
                                    TextEn = v.Text?.En ?? string.Empty,
                                    Juz = v.Juz,
                                    Page = v.Page
                                }).ToList() ?? new List<Verse>()
                            };
                            surahs.Add(surah);
                        }
                        await context.Surahs.AddRangeAsync(surahs);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        private static List<DailyInspiration> GetDefaultInspirations()
        {
            return new List<DailyInspiration>
            {
                new DailyInspiration { Type = "Ayah", Content = "إِنَّ مَعَ الْعُسْرِ يُسْرًا", Source = "سورة الشرح - 6" },
                new DailyInspiration { Type = "Ayah", Content = "وَقُل رَّبِّ زِدْنِي عِلْمًا", Source = "سورة طه - 114" },
                new DailyInspiration { Type = "Ayah", Content = "لَا يُكَلِّفُ اللَّهُ نَفْسًا إِلَّا وُسْعَهَا", Source = "سورة البقرة - 286" },
                new DailyInspiration { Type = "Ayah", Content = "فَاصْبِرْ صَبْرًا جَمِيلًا", Source = "سورة المعارج - 5" },
                new DailyInspiration { Type = "Ayah", Content = "وَتَوَكَّلْ عَلَى اللَّهِ ۚ وَكَفَىٰ بِاللَّهِ وَكِيلًا", Source = "سورة الأحزاب - 3" },
                new DailyInspiration { Type = "Hadith", Content = "إِنَّمَا الأَعْمَالُ بِالنِّيَّاتِ", Source = "البخاري ومسلم" },
                new DailyInspiration { Type = "Hadith", Content = "الكلمة الطيبة صدقة", Source = "البخاري ومسلم" },
                new DailyInspiration { Type = "Dhikr", Content = "سبحان الله وبحمده", Source = "أذكار" },
                new DailyInspiration { Type = "Dhikr", Content = "أستغفر الله وأتوب إليه", Source = "أذكار" }
            };
        }
    }

    // JSON DTOs
    public class JsonSurah
    {
        public int Number { get; set; }
        public JsonName? Name { get; set; }
        public int Verses_Count { get; set; }
        public List<JsonVerse>? Verses { get; set; }
    }

    public class JsonName
    {
        public string? Ar { get; set; }
        public string? En { get; set; }
        public string? Transliteration { get; set; }
    }

    public class JsonVerse
    {
        public int Number { get; set; }
        public JsonText? Text { get; set; }
        public int Juz { get; set; }
        public int Page { get; set; }
    }

    public class JsonText
    {
        public string? Ar { get; set; }
        public string? En { get; set; }
    }
}
