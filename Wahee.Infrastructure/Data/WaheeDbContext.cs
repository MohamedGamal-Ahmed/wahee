using Microsoft.EntityFrameworkCore;
using Wahee.Core.Models;

namespace Wahee.Infrastructure.Data
{
    public class WaheeDbContext : DbContext
    {
        public DbSet<DailyInspiration> DailyInspirations { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<WidgetState> WidgetStates { get; set; }
        public DbSet<Radio> Radios { get; set; }
        public DbSet<Surah> Surahs { get; set; }
        public DbSet<Verse> Verses { get; set; }

        public string DbPath { get; }

        public WaheeDbContext()
        {
            // FIXED: DB path to AppData for stable deployment permissions
            DbPath = GetDbPath();
        }

        private static string GetDbPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = Path.Combine(appData, "Wahee");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "wahee.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSetting>().HasKey(s => s.Key);
            modelBuilder.Entity<WidgetState>().HasKey(w => w.WidgetName);
            modelBuilder.Entity<Radio>().HasKey(r => r.Id);

            // Seed initial data if needed
            base.OnModelCreating(modelBuilder);
        }
    }
}
