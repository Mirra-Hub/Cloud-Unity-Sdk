using System;
using JetBrains.Annotations;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.Dto
{
    [Serializable]
    public sealed class RuleNodeDto
    {
        public LogicalOperator LogicalOperator { get; set; }
        [CanBeNull] public RuleNodeDto[] Children { get; set; } = null;
        
        public SourceType Type { get; set; }
        [CanBeNull] public string Data { get; set; } = "{}";
    }
}