using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers
{
    public interface IRuleParser
    {
        public BaseRule Parse(string jsonData);
    }
}