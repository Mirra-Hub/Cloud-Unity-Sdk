using System;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Construction
{
    public sealed class RulesTreeBuilder
    {
        private IRuleNode _root;

        public IRuleNode Build() => _root;
        
        public AndBuilder And(Action<AndBuilder> configure)
        {
            var builder = new AndBuilder();
            configure(builder);
            _root = builder.Build();
            return builder;
        }
        
        public OrBuilder Or(Action<OrBuilder> configure)
        {
            var builder = new OrBuilder();
            configure(builder);
            _root = builder.Build();
            return builder;
        }
    }
}