using System;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers.ParsedDataModels
{
    [Serializable]
    public sealed class ActiveDaysRuleData
    {
        public RuleOperator Operator { get; set; }
        public int DaysCount { get; set; }
    }
}