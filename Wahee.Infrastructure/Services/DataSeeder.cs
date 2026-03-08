using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Wahee.Core.Models;
using Wahee.Infrastructure.Data;

namespace Wahee.Infrastructure.Services
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(WaheeDbContext context)
        {
            if (!context.DailyInspirations.Any())
            {
                var inspirations = GetDefaultInspirations();
                await context.DailyInspirations.AddRangeAsync(inspirations);
                await context.SaveChangesAsync();
            }

            if (!context.Surahs.Any())
            {
                var jsonPath = ResolveDatabaseJsonPath();
                if (!string.IsNullOrWhiteSpace(jsonPath) && File.Exists(jsonPath))
                {
                    await using var openStream = File.OpenRead(jsonPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var jsonSurahs = await JsonSerializer.DeserializeAsync<List<JsonSurah>>(openStream, options);

                    if (jsonSurahs != null)
                    {
                        var surahs = new List<Surah>();
                        foreach (var jsonSurah in jsonSurahs)
                        {
                            var surah = new Surah
                            {
                                Number = jsonSurah.Number,
                                NameAr = jsonSurah.Name?.Ar ?? string.Empty,
                                NameEn = jsonSurah.Name?.En ?? string.Empty,
                                Transliteration = jsonSurah.Name?.Transliteration ?? string.Empty,
                                VersesCount = jsonSurah.Verses_Count,
                                Verses = jsonSurah.Verses?.Select(v => new Verse
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

        private static string? ResolveDatabaseJsonPath()
        {
            var candidates = new List<string>
            {
                Path.Combine(AppContext.BaseDirectory, "database.json"),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "database.json")),
                Path.Combine(Directory.GetCurrentDirectory(), "database.json")
            };

            return candidates.FirstOrDefault(File.Exists);
        }

        private static List<DailyInspiration> GetDefaultInspirations()
        {
            return new List<DailyInspiration>
            {
                new() { Type = "Ayah", Content = "إِنَّ مَعَ الْعُسْرِ يُسْرًا", Source = "سورة الشرح - 6" },
                new() { Type = "Ayah", Content = "وَقُل رَّبِّ زِدْنِي عِلْمًا", Source = "سورة طه - 114" },
                new() { Type = "Ayah", Content = "لَا يُكَلِّفُ اللَّهُ نَفْسًا إِلَّا وُسْعَهَا", Source = "سورة البقرة - 286" },
                new() { Type = "Ayah", Content = "فَاصْبِرْ صَبْرًا جَمِيلًا", Source = "سورة المعارج - 5" },
                new() { Type = "Ayah", Content = "وَتَوَكَّلْ عَلَى اللَّهِ وَكَفَىٰ بِاللَّهِ وَكِيلًا", Source = "سورة الأحزاب - 3" },
                new() { Type = "Hadith", Content = "إِنَّمَا الْأَعْمَالُ بِالنِّيَّاتِ", Source = "البخاري ومسلم" },
                new() { Type = "Hadith", Content = "الكلمة الطيبة صدقة", Source = "البخاري ومسلم" },
                new() { Type = "Dhikr", Content = "سبحان الله وبحمده", Source = "أذكار" },
                new() { Type = "Dhikr", Content = "أستغفر الله وأتوب إليه", Source = "أذكار" }
            };
        }
    }

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
