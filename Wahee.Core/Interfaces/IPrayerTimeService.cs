using System.Collections.Generic;
using System.Threading.Tasks;

namespace Wahee.Core.Interfaces
{
    public interface IPrayerTimeService
    {
        Task<Dictionary<string, string>> GetPrayerTimesAsync(string city, string country);
        Task<(string City, string Country)> GetLocationByIpAsync();
        void PlayAdhan();
    }
}
