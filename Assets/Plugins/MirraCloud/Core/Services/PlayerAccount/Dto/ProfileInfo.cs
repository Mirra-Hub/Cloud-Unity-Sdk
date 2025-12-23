using System;
using MirraCloud.Json;
using MirraCloud.Core.Auth;

namespace Plugins.MirraCloud.Core.Services.PlayerAccount.Dto
{
    [Serializable]
    public class ProfileInfo
    {
        [JsonNameCamel] public string Id;
        [JsonNameCamel] public string AccountId;
        [JsonNameCamel] public string Nickname;
        [JsonName("iconKey")] public IconKeyDto IconKey;
        [JsonNameCamel] public string Status;
        [JsonNameCamel] public string[] SegmentIds;
        [JsonNameCamel] public DateTime LastLogin;
        [JsonNameCamel] public DateTime CreatedDate;
        [JsonNameCamel] public DateTime UpdatedDate;
    }
}
