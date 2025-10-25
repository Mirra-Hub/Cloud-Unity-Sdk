using System.Linq;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.LogicalNodes
{
    public sealed class OrNode : BaseLogicalNode
    {
        public override bool Execute() => Children.Any(c => c.Execute());
    }
}