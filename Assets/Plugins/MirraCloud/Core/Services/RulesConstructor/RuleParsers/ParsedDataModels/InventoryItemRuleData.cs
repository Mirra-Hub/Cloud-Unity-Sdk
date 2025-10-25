using System;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers.ParsedDataModels
{
    [Serializable]
    public sealed class InventoryItemRuleData
    {
        public string ItemId { get; set; } = null!;
        public RuleOperator Operator { get; set; }
        public int Count { get; set; }
    }
}