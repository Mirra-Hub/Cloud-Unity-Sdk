using System;
using MirraCloud.Json;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class AccountDto
    {
        [JsonNameCamel] public string Id;
        [JsonNameCamel] public string Nickname;
        [JsonNameCamel] public int Age;
        [JsonName("iconKey")] public string IconKeyJson;
        [JsonNameCamel] public string Country;
        [JsonNameCamel] public string LanguageCode;
        [JsonNameCamel] public string TimeZone;
        [JsonNameCamel] public string[] SegmentIds;
        [JsonNameCamel] public string Status;
        [JsonNameCamel] public DateTime LastLoginDate;
        [JsonNameCamel] public DateTime CreatedDate;
        [JsonNameCamel] public DateTime UpdatedDate;
        [JsonNameCamel] public int TotalActiveDays;
        [JsonNameCamel] public int ConsecutiveActiveDays;
        [JsonNameCamel] public int MaxConsecutiveActiveDays;
        [JsonNameCamel] public int TotalSessions;
    }
}
