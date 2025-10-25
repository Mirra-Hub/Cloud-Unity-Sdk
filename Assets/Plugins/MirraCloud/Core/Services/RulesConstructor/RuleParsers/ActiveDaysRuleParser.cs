using Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers.ParsedDataModels;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.RuleNodes;
using UnityEngine;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers
{
    public class ActiveDaysRuleParser : IRuleParser
    {
        public BaseRule Parse(string jsonData)
        {
            var data = JsonUtility.FromJson<ActiveDaysRuleData>(jsonData);
            return new ActiveDaysRule(data);
        }
    }
}