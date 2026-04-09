using System;
using MirraCloud.Core.Enums;

namespace MirraCloud.Core.Auth
{
    public class PlayerAccountInfo
    {
        public string Id { get; private set; }
        public string Nickname { get; set; }
        public int Age { get; private set; }
        public IconKeyDto IconKey { get; private set; }
        public CountryCode Country { get; private set; }
        public LanguageCode LanguageCode { get; private set; }
        public string TimeZone { get; private set; }
        public string Status { get; private set; }
        public string[] SegmentIds { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime UpdatedDate { get; private set; }
        public DateTime LastLoginDate { get; private set; }
        
        public int TotalActiveDays { get; private set; }
        public int ConsecutiveActiveDays { get; private set; }
        public int MaxConsecutiveActiveDays { get; private set; }
        public int TotalSessions { get; private set; }

        public PlayerAccountInfo(AccountDto dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            Id = dto.Id;
            Nickname = dto.Nickname;
            Age = dto.Age;
            IconKey = dto.IconKey;
            Country = dto.Country;
            LanguageCode = dto.LanguageCode;
            TimeZone = dto.TimeZone;
            Status = dto.Status;
            SegmentIds = dto.SegmentIds ?? Array.Empty<string>();
            CreatedDate = dto.CreatedDate;
            UpdatedDate = dto.UpdatedDate;
            LastLoginDate = dto.LastLoginDate;
            TotalActiveDays = dto.TotalActiveDays;
            ConsecutiveActiveDays = dto.ConsecutiveActiveDays;
            MaxConsecutiveActiveDays = dto.MaxConsecutiveActiveDays; 
            TotalSessions = dto.TotalSessions;
        }
    }
}
