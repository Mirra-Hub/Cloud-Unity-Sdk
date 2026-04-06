using System;
using MirraCloud.Json;

namespace MirraCloud.Core.Localization.Dto
{
    [Serializable]
    public sealed class LocalizationValueDto
    {
        [JsonNameCamel] public int LanguageCode;
        [JsonNameCamel] public string LanguageString;
        [JsonNameCamel] public string Value;
    }
}
