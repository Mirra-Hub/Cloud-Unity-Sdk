using System;
using MirraCloud.Json;

namespace Plugins.MirraCloud.Core.Services.PlayerAccount.Dto
{
    [Serializable]
    public class UpdateProfileStatusDto
    {
        [JsonNameCamel] public string Status;
    }
}

