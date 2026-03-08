// Task Summary:
// - Phase 2 / Task 6: Centralized prayer keys and Arabic names.
namespace Wahee.Core.Constants
{
    public static class PrayerConstants
    {
        public static readonly string[] Keys = { "Fajr", "Dhuhr", "Asr", "Maghrib", "Isha" };
        public static readonly string[] NamesAr = { "الفجر", "الظهر", "العصر", "المغرب", "العشاء" };

        public static readonly Dictionary<string, string> KeyToArabic = new()
        {
            { "Fajr", "الفجر" },
            { "Dhuhr", "الظهر" },
            { "Asr", "العصر" },
            { "Maghrib", "المغرب" },
            { "Isha", "العشاء" }
        };
    }
}
