using System.Collections.Generic;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Dto;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers.Construction
{
    public static class RuleFactory
    {
        private static readonly Dictionary<SourceType, IRuleParser> Parsers = new();
        
        static RuleFactory()
        {
            Parsers.Add(SourceType.InventoryItem, new InventoryItemRuleParser());
            Parsers.Add(SourceType.InventoryCurrency, new InventoryCurrencyRuleParser());
            Parsers.Add(SourceType.ActiveDays, new ActiveDaysRuleParser());
        }
        
        public static BaseRule CreateRule(RuleNodeDto ruleDto)
        {
            var parser = Parsers[ruleDto.Type];
            return parser.Parse(ruleDto.Data);
        }
    }
}