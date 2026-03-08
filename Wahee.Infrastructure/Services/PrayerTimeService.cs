using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Wahee.Core.Interfaces;

namespace Wahee.Infrastructure.Services
{
    public class PrayerTimeService : IPrayerTimeService
    {
        private readonly HttpClient _httpClient;
        private const string IpApiUrl = "http://ip-api.com/json";
        private const string AladhanApiUrl = "http://api.aladhan.com/v1/timingsByCity";

        public PrayerTimeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(string City, string Country)> GetLocationByIpAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<IpApiResponse>(IpApiUrl);
                if (response != null && response.Status == "success")
                {
                    return (response.City ?? "Cairo", response.Country ?? "Egypt");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching IP location: {ex.Message}");
            }
            return ("Cairo", "Egypt");
        }

        public async Task<Dictionary<string, string>> GetPrayerTimesAsync(string city, string country)
        {
            try
            {
                string url = $"{AladhanApiUrl}?city={city}&country={country}&method=5"; // Method 5 = Egyptian General Authority of Survey
                var response = await _httpClient.GetFromJsonAsync<AladhanResponse>(url);
                if (response?.Data?.Timings != null)
                {
                    return response.Data.Timings;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching prayer times: {ex.Message}");
            }
            return new Dictionary<string, string>();
        }

        public void PlayAdhan()
        {
            // Placeholder for future adhan sound implementation
            // In the future: new System.Media.SoundPlayer("Assets/Audio/Adhan.wav").Play();
            Console.WriteLine("Adhan time! PlayAdhan() called.");
        }

        private class IpApiResponse
        {
            public string? Status { get; set; }
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
