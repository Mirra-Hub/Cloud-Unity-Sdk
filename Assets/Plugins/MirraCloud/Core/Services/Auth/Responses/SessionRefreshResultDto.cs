using System;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class SessionRefreshResultDto
    {
        public string AccountId;
        public string ProjectId;
        public SessionInfoDto Session;
    }
}

