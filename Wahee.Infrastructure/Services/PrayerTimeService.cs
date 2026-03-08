using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Wahee.Core.Interfaces;

namespace Wahee.Infrastructure.Services
{
    public class PrayerTimeService : IPrayerTimeService
    {
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;

        private const string IpApiUrl = "https://ipwho.is/";
        private const string AladhanApiUrl = "https://api.aladhan.com/v1/timingsByCity";

        public PrayerTimeService(HttpClient httpClient, ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _settingsService = settingsService;
        }

        public async Task<(string City, string Country)> GetLocationByIpAsync()
        {
            try
            {
                var autoDetectRaw = await _settingsService.GetSettingAsync("AutoDetectLocation");
                var autoDetect = !bool.TryParse(autoDetectRaw, out var parsed) || parsed;

                if (!autoDetect)
                {
                    var manualCity = await _settingsService.GetSettingAsync("ManualCity");
                    var manualCountry = await _settingsService.GetSettingAsync("ManualCountry");

                    if (!string.IsNullOrWhiteSpace(manualCity) && !string.IsNullOrWhiteSpace(manualCountry))
                    {
                        return (manualCity.Trim(), manualCountry.Trim());
                    }
                }

                var response = await _httpClient.GetFromJsonAsync<IpWhoResponse>(IpApiUrl);
                if (response != null && response.Success)
                {
                    return (response.City ?? "Cairo", response.Country ?? "Egypt");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching location: {ex.Message}");
            }

            return ("Cairo", "Egypt");
        }

        public async Task<Dictionary<string, string>> GetPrayerTimesAsync(string city, string country)
        {
            try
            {
                var url = $"{AladhanApiUrl}?city={city}&country={country}&method=5";
                var response = await _httpClient.GetFromJsonAsync<AladhanResponse>(url);

                if (response?.Data?.Timings != null)
                {
                    return await ApplySeasonModeAsync(response.Data.Timings);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching prayer times: {ex.Message}");
            }

            return new Dictionary<string, string>();
        }

        private async Task<Dictionary<string, string>> ApplySeasonModeAsync(Dictionary<string, string> timings)
        {
            var adjusted = new Dictionary<string, string>();
            var seasonMode = (await _settingsService.GetSettingAsync("SeasonMode") ?? "Auto").Trim();

            var offsetHours = seasonMode.Equals("Summer", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            foreach (var item in timings)
            {
                var normalized = NormalizeTime(item.Value);
                if (DateTime.TryParseExact(normalized, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                {
                    var adjustedTime = parsed.AddHours(offsetHours);
                    adjusted[item.Key] = adjustedTime.ToString("HH:mm", CultureInfo.InvariantCulture);
                }
                else
                {
                    adjusted[item.Key] = item.Value;
                }
            }

            return adjusted;
        }

        private static string NormalizeTime(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "00:00";
            }

            var trimmed = value.Trim();
            return trimmed.Length >= 5 ? trimmed.Substring(0, 5) : trimmed;
        }

        public void PlayAdhan()
        {
            Console.WriteLine("Adhan time! PlayAdhan() called.");
        }

        private class IpWhoResponse
        {
            public bool Success { get; set; }
            public string? City { get; set; }
            public string? Country { get; set; }
        }

        private class AladhanResponse
        {
            public AladhanData? Data { get; set; }
        }

        private class AladhanData
        {
            public Dictionary<string, string>? Timings { get; set; }
        }
    }
}
