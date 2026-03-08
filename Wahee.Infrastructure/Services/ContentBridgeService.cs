using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Wahee.Core.Interfaces;
using Wahee.Core.Models;

namespace Wahee.Infrastructure.Services
{

    public class ContentBridgeService : IContentBridgeService
    {
        private readonly HttpClient _httpClient;
        private const string AlQuranApiUrl = "https://api.alquran.cloud/v1/ayah";

        public ContentBridgeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetTafsirAsync(int surahNumber, int verseNumber)
        {
            try
            {
                var url = $"{AlQuranApiUrl}/{surahNumber}:{verseNumber}/ar.muyassar";
                var response = await _httpClient.GetFromJsonAsync<AlQuranResponse>(url);
                return response?.Data?.Text ?? "تعذر تحميل التفسير حالياً.";
            }
            catch { return "حدث خطأ أثناء جلب التفسير."; }
        }

        public async Task<string> GetAudioUrlAsync(int surahNumber, int verseNumber)
        {
            try
            {
                // Specifically Alafasy: ar.alafasy
                var url = $"{AlQuranApiUrl}/{surahNumber}:{verseNumber}/ar.alafasy";
                var response = await _httpClient.GetFromJsonAsync<AlQuranAudioResponse>(url);
                return response?.Data?.Audio ?? "";
            }
            catch { return ""; }
        }

        private class AlQuranAudioResponse
        {
            public AudioData? Data { get; set; }
        }

        private class AudioData
        {
            public string? Audio { get; set; }
        }

        // Response models for AlQuran.cloud
        private class AlQuranResponse
        {
            public AlQuranData? Data { get; set; }
        }

        private class AlQuranData
        {
            public string? Text { get; set; }
            public int Number { get; set; }
        }
    }
}
