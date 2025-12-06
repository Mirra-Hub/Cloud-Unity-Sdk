using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class AccountDto
    {
        public string Id;
        public string Nickname;
        public int Age;
        public string IconKeyJson;
        public string Country;
        public string LanguageCode;
        public string TimeZone;
        public string[] SegmentIds;
        public string Status;
        public DateTime LastLoginDate;
        public DateTime CreatedDate;
        public DateTime UpdatedDate;
    }
}

