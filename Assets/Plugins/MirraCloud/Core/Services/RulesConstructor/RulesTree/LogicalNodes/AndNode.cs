using System.Linq;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.LogicalNodes
{
    public sealed class AndNode : BaseLogicalNode
    {
        public override bool Execute() => Children.All(c => c.Execute());
    }
}