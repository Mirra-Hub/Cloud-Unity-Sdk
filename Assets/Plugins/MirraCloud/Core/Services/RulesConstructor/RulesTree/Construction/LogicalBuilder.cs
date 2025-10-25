using System;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Construction
{
    public abstract class LogicalBuilder<TNode, TBuilder>
        where TNode : BaseLogicalNode, new()
        where TBuilder : LogicalBuilder<TNode, TBuilder>
    {
        protected readonly TNode Node = new();

        public TNode Build() => Node;
        
        public TBuilder AddChild(IRuleNode node)
        {
            Node.AddChild(node);
            return (TBuilder)this;
        }
        
        public TBuilder Rule(Func<BaseRule> configure)
        {
            Node.AddChild(configure());
            return (TBuilder)this;
        }

        public TBuilder And(Action<AndBuilder> configure)
        {
            var builder = new AndBuilder();
            configure(builder);
            Node.AddChild(builder.Build());
            return (TBuilder)this;
        }
        
        public TBuilder Or(Action<OrBuilder> configure)
        {
            var builder = new OrBuilder();
            configure(builder);
            Node.AddChild(builder.Build());
            return (TBuilder)this;
        }
    }
}