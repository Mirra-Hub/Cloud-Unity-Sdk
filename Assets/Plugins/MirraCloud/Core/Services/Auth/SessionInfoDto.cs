using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class SessionInfoDto
    {
        public string SessionId;
        public string RefreshToken;
        public DateTime ExpiresAt;
    }
}

