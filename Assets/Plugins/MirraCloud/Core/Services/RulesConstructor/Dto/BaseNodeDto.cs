using System;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;
using UnityEngine;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor.Dto
{
    [Serializable]
    public abstract class BaseNodeDto
    {
        public SourceType sourceType;

        public abstract bool Execute();
    }
    
    [Serializable]
    public sealed class OrNodeDto : BaseNodeDto
    {
        public BaseNodeDto[] children;
        
        public override bool Execute()
        {
            Debug.Log("or node execute");
            foreach (var child in children)
            {
                if (child.Execute() == true)
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    [Serializable]
    public sealed class AndNodeDto : BaseNodeDto
    {
        public BaseNodeDto[] children;
        
        public override bool Execute()
        {
            Debug.Log("and node execute");

            foreach (var child in children)
            {
                if (child.Execute() == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
    
    [Serializable]
    public sealed class DayActiveNodeDto : BaseNodeDto
    {
        public RuleOperator @operator;
        public int daysCount;
        
        public override bool Execute()
        {
            Debug.Log("day active node execute");
            
            return true;
        }
    }
    
    [Serializable]
    public sealed class InventoryItemNodeDto : BaseNodeDto
    {
        public RuleOperator @operator;
        public string itemId;
        public int itemCount;
        
        public override bool Execute()
        {
            Debug.Log("inventory item  node execute");
            return true;
        }
    }
}