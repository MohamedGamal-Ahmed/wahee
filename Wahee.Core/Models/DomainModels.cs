namespace Wahee.Core.Models
{
    public class DailyInspiration
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Ayah, Hadith, Dhikr
        public string Content { get; set; } = string.Empty;
        public string? Source { get; set; }
        public DateTime LastShown { get; set; }
    }

    public class UserSetting
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class WidgetState
    {
        public string WidgetName { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class Radio
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class Surah
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string Transliteration { get; set; } = string.Empty;
        public int VersesCount { get; set; }
        public List<Verse> Verses { get; set; } = new();
    }

    public class Verse
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string TextAr { get; set; } = string.Empty;
        public string TextEn { get; set; } = string.Empty;
        public int Juz { get; set; }
        public int Page { get; set; }
        public int SurahId { get; set; }
        public Surah? Surah { get; set; }
    }
}
