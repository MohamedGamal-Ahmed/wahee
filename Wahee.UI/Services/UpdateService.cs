using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Wahee.Core.Interfaces;

namespace Wahee.UI.Services
{
    public interface IUpdateService
    {
        Task CheckForUpdatesAsync(bool isManualCheck);
    }

    public class UpdateService : IUpdateService
    {
        private const string Owner = "MohamedGamal-Ahmed";
        private const string Repo = "wahee";
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;

        public UpdateService(HttpClient httpClient, ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _settingsService = settingsService;

            if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WaheeApp/1.0");
            }
        }

        public async Task CheckForUpdatesAsync(bool isManualCheck)
        {
            try
            {
                var autoUpdateRaw = await _settingsService.GetSettingAsync("AutoUpdateEnabled");
                var autoUpdateEnabled = !bool.TryParse(autoUpdateRaw, out var parsed) || parsed;

                if (!isManualCheck && !autoUpdateEnabled)
                {
                    return;
                }

                var apiUrl = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
                var release = await _httpClient.GetFromJsonAsync<GitHubRelease>(apiUrl);
                if (release == null || string.IsNullOrWhiteSpace(release.TagName))
                {
                    return;
                }

                var currentVersion = GetCurrentVersion();
                var latestVersion = ParseVersion(release.TagName);
                if (latestVersion <= currentVersion)
                {
                    if (isManualCheck)
                    {
                        MessageBox.Show("أنت تستخدم أحدث إصدار حالياً.", "وحي - التحديثات", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return;
                }

                var installerAsset = release.Assets?.FirstOrDefault(a =>
                    a.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
                    a.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase));

                var downloadUrl = installerAsset?.BrowserDownloadUrl;
                if (string.IsNullOrWhiteSpace(downloadUrl))
                {
                    var openReleasePage = MessageBox.Show(
                        $"إصدار جديد متاح: {release.TagName}\nهل تريد فتح صفحة التحميل؟",
                        "وحي - تحديث جديد",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (openReleasePage == MessageBoxResult.Yes)
                    {
                        OpenUrl(release.HtmlUrl);
                    }

                    return;
                }

                var result = MessageBox.Show(
                    $"إصدار جديد متاح: {release.TagName}\nهل تريد تنزيله وتثبيته الآن؟",
                    "وحي - تحديث جديد",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                var tempPath = Path.Combine(Path.GetTempPath(), installerAsset!.Name);
                await using (var stream = await _httpClient.GetStreamAsync(downloadUrl))
                await using (var file = File.Create(tempPath))
                {
                    await stream.CopyToAsync(file);
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateService] Update check failed: {ex.Message}");

                if (isManualCheck)
                {
                    MessageBox.Show("تعذر التحقق من التحديثات حالياً.", "وحي - التحديثات", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private static Version GetCurrentVersion()
        {
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return v ?? new Version(1, 0, 0, 0);
        }

        private static Version ParseVersion(string value)
        {
            var clean = value.Trim().TrimStart('v', 'V');
            return Version.TryParse(clean, out var parsed) ? parsed : new Version(0, 0, 0, 0);
        }

        private static void OpenUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private sealed class GitHubRelease
        {
            public string TagName { get; set; } = string.Empty;
            public string HtmlUrl { get; set; } = string.Empty;
            public GitHubAsset[] Assets { get; set; } = Array.Empty<GitHubAsset>();
        }

        private sealed class GitHubAsset
        {
            public string Name { get; set; } = string.Empty;
            public string BrowserDownloadUrl { get; set; } = string.Empty;
        }
    }
}
