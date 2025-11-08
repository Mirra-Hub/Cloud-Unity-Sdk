using System;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.Dto
{
    [Serializable]
    public abstract class BaseNodeDto
    {
        public SourceType sourceType;
    }
    
    [Serializable]
    public sealed class OrNodeDto : BaseNodeDto
    {
        public BaseNodeDto[] children;
    }
    
    [Serializable]
    public sealed class AndNodeDto : BaseNodeDto
    {
        public BaseNodeDto[] children;
    }
    
    [Serializable]
    public sealed class DayActiveNodeDto : BaseNodeDto
    {
        public RuleOperator @operator;
        public int daysCount;
    }
    
    [Serializable]
    public sealed class InventoryItemNodeDto : BaseNodeDto
    {
        public RuleOperator @operator;
        public string itemId;
        public int itemCount;
    }
}