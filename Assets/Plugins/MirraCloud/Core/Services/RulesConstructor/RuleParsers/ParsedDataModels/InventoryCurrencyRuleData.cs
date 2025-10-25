using System;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers.ParsedDataModels
{
    [Serializable]
    public sealed class InventoryCurrencyRuleData
    { 
        public string CurrencyId { get; set; } = null!;
        public RuleOperator Operator { get; set; }
        public int CurrencyCount { get; set; }
    }
}