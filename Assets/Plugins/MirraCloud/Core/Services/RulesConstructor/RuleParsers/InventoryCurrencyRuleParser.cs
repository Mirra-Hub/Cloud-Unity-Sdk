using Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers.ParsedDataModels;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.RuleNodes;
using UnityEngine;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers
{
    public class InventoryCurrencyRuleParser : IRuleParser
    {
        public BaseRule Parse(string jsonData)
        {
            var data = JsonUtility.FromJson<InventoryCurrencyRuleData>(jsonData);
            return new InventoryCurrencyRule(data);
        }
    }
}