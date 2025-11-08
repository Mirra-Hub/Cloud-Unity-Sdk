using System;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.Dto
{
    [Serializable]
    public sealed class RulesTreeDto
    {
        public string id { get; set; } = null!;
        public BaseNodeDto root { get; set; } = null!;
    }
}