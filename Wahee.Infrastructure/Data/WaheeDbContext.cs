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
            var folder = AppDomain.CurrentDomain.BaseDirectory;
            DbPath = Path.Combine(folder, "wahee.db");
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
