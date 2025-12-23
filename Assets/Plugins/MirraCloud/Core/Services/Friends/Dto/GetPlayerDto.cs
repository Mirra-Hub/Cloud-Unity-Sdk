using System;
using MirraCloud.Json;

namespace MirraCloud.Core.Friends.Dto
{
    [Serializable]
    public class GetPlayerDto
    {
        [JsonNameCamel] public string PlayerId;
    }
}

