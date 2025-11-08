using System;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Dto;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RuleParsers.Construction;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Abstractions;
using Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.LogicalNodes;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.RulesTree.Construction
{
    public static class RulesTreeFactory
    {
        /*public static IRuleNode CreateFrom(RulesTreeDto dto)
        {
            return dto is not null
                ? GetRuleNode(dto.Root)
                : throw new ArgumentNullException(nameof(dto));
        }

        private static IRuleNode GetRuleNode(RuleNodeDto dto)
        {
            var hasChildren = dto.Children is { Length: > 0 };
            var hasData = !string.IsNullOrWhiteSpace(dto.Data) && dto.Data != "{}";

            if (hasChildren && hasData)
            {
                throw new InvalidOperationException("Invalid node: node can be only logical or only rule!");
            }

            if (hasData)
            {
                return RuleFactory.CreateRule(dto);
            }

            if (!hasChildren)
            {
                throw new InvalidOperationException("Invalid node: node does not contain any data!");
            }
            
            return dto.LogicalOperator switch
            {
                LogicalOperator.And => BuildLogicalNode<AndNode, AndBuilder>(new AndBuilder(), dto.Children),
                LogicalOperator.Or  => BuildLogicalNode<OrNode, OrBuilder>(new OrBuilder(), dto.Children),
                
                _ => throw new ArgumentOutOfRangeException(nameof(dto.LogicalOperator), dto.LogicalOperator, "")
            };
        }

        private static IRuleNode BuildLogicalNode<TNode, TBuilder>(TBuilder builder, RuleNodeDto[] children)
            where TNode : BaseLogicalNode, new()
            where TBuilder : LogicalBuilder<TNode, TBuilder>
        {
            foreach (var childDto in children)
            {
                builder.AddChild(GetRuleNode(childDto));
            }

            return builder.Build();
        }*/
    }
}
