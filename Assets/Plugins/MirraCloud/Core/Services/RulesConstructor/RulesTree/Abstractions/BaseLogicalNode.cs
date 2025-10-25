using System.Collections.Generic;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions
{
    public abstract class BaseLogicalNode : IRuleNode
    {
        protected readonly List<IRuleNode> Children = new();
        
        public abstract bool Execute();
        
        public void AddChild(IRuleNode node) => Children.Add(node);
    }
}