using System;
using MirraCloud.Json;

namespace MirraCloud.Core.Auth
{
    [Serializable]
    public class SessionRefreshResultDto
    {
        [JsonNameCamel] public string AccountId;
        [JsonNameCamel] public string ProjectId;
        [JsonNameCamel] public SessionInfoDto Session;
    }
}
