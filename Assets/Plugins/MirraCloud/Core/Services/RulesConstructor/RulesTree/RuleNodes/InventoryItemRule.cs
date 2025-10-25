using System;
using System.Linq;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers.ParsedDataModels;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.RuleNodes
{
    public sealed class InventoryItemRule : BaseRule
    {
        private readonly InventoryItemRuleData _data;

        public InventoryItemRule(InventoryItemRuleData data)
        {
            Type = SourceType.InventoryItem;
            _data = data;
        }

        public override bool Execute()
        {
            var count = MirraCloudSDK.Economy.Items
                .Where(i => i.ResourceId == _data.ItemId)
                .ToList()
                .Count;
            
            return _data.Operator switch
            {
                RuleOperator.Eq => count == _data.Count,
                RuleOperator.Gt => count > _data.Count,
                RuleOperator.Lt => count < _data.Count,
                RuleOperator.Ge => count >= _data.Count,
                RuleOperator.Le => count <= _data.Count,
                RuleOperator.Ne => count != _data.Count,
                
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}