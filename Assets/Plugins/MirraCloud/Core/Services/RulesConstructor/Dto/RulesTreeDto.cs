using System;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.Dto
{
    [Serializable]
    public sealed class RulesTreeDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public RuleNodeDto Root { get; set; } = null!;
    }
}