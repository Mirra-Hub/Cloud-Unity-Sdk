using System;
using System.Collections.Generic;

namespace MirraCloud.Core.Auth
{
    /// <summary>
    /// DTO for resolving a provider link conflict.
    /// Platform-specific fields are populated only when ProviderType corresponds to AuthProviderType.Platform on the server.
    /// </summary>
    [Serializable]
    public class LinkAuthProviderDto
    {
        public int ProviderType;
        public string TargetAccountId;

        public string PlatformId;
        public string ExternalUserId;
        public string AuthCode;
        public string PlatformToken;
        public Dictionary<string, string> Extra;
    }
}
