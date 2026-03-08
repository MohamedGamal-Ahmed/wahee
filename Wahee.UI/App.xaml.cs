using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Wahee.Core.Interfaces;
using Wahee.Infrastructure.Data;
using Wahee.Infrastructure.Services;
using System.Net.Http;

namespace Wahee.UI
{
    public partial class App : Application
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                var serviceCollection = new ServiceCollection();
                ConfigureServices(serviceCollection);

                ServiceProvider = serviceCollection.BuildServiceProvider();

                base.OnStartup(e);

                var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();

                // Run background initialization
                InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"FATAL ERROR: {ex}", "Wahee Crash", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private async void InitializeAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    using (var scope = ServiceProvider!.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<WaheeDbContext>();
                        context.Database.EnsureCreated();
                        await DataSeeder.SeedAsync(context);
                        
                        var quranService = scope.ServiceProvider.GetRequiredService<IQuranDataService>();
                        await quranService.RefreshRadiosAsync();
                    }
                });

                // Initialize Adhan notification service
                var adhanService = ServiceProvider!.GetRequiredService<AdhanNotificationService>();
                adhanService.AdhanTriggered += AdhanService_AdhanTriggered;
                await adhanService.InitializeAsync();
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => 
                    MessageBox.Show($"Startup Error: {ex.Message}", "Wahee Error", MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        private readonly System.Windows.Media.MediaPlayer _adhanPlayer = new();
        private void AdhanService_AdhanTriggered(object? sender, string prayerName)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    string adhanPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "1027.mp3");
                    if (System.IO.File.Exists(adhanPath))
                    {
                        _adhanPlayer.Open(new Uri(adhanPath));
                        _adhanPlayer.Play();
                    }
                    else 
                    {
                         // Check fallback in Assets
                        string fallbackPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Audio", "1027.mp3");
                        if (System.IO.File.Exists(fallbackPath))
                        {
                            _adhanPlayer.Open(new Uri(fallbackPath));
                            _adhanPlayer.Play();
                        }
                    }
                    
                    // Show a notification
                    var mainWindow = ServiceProvider?.GetService<MainWindow>();
                    mainWindow?.Dispatcher.Invoke(() => {
                        // You could add a UI notification here later if needed
                        System.Diagnostics.Debug.WriteLine($"Playing Adhan for {prayerName}");
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Adhan Playback Error: {ex.Message}");
                }
            });
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddDbContext<WaheeDbContext>();
            services.AddScoped<IInspirationRepository, InspirationRepository>();
            services.AddSingleton<IWallpaperService, WallpaperService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IWidgetSettingsService, WidgetSettingsService>();
            services.AddScoped<IQuranDataService, QuranDataService>();
            services.AddScoped<IPrayerTimeService, PrayerTimeService>();
            services.AddScoped<IContentBridgeService, ContentBridgeService>();
            services.AddScoped<IFavoritesService, FavoritesService>();
            
            // Adhan Notifications
            services.AddSingleton<AdhanNotificationService>();
            
            // ViewModels and Views
            services.AddTransient<MainWindow>();
        }
    }
}
