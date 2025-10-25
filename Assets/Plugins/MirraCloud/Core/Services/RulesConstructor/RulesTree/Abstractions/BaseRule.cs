using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions
{
    public abstract class BaseRule : IRuleNode
    {
        public SourceType Type { get; protected set; }
        public abstract bool Execute();
    }
}