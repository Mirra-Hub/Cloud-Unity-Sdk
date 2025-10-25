using System;
using System.Linq;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers.ParsedDataModels;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.RuleNodes
{
    public sealed class ActiveDaysRule : BaseRule
    {
        private readonly ActiveDaysRuleData _data;
        
        public ActiveDaysRule(ActiveDaysRuleData data)
        {
            Type = SourceType.ActiveDays;
            _data = data;
        }
        
        public override bool Execute()
        {
            throw new NotImplementedException();
        }
    }
}