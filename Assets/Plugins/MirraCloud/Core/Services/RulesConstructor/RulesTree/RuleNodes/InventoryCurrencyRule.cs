using System;
using System.Linq;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers.ParsedDataModels;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.RuleNodes
{
    public sealed class InventoryCurrencyRule : BaseRule
    {
        private readonly InventoryCurrencyRuleData _data;
        
        public InventoryCurrencyRule(InventoryCurrencyRuleData data)
        {
            Type = SourceType.InventoryCurrency;
            _data = data;
        }
        
        public override bool Execute()
        {
            var count = MirraCloudSDK.Economy.Currencies
                .Where(c => c.ResourceId == _data.CurrencyId)
                .ToList()
                .Count;

            return _data.Operator switch
            {
                RuleOperator.Eq => count == _data.CurrencyCount,
                RuleOperator.Gt => count > _data.CurrencyCount,
                RuleOperator.Lt => count < _data.CurrencyCount,
                RuleOperator.Ge => count >= _data.CurrencyCount,
                RuleOperator.Le => count <= _data.CurrencyCount,
                RuleOperator.Ne => count != _data.CurrencyCount,
                
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}