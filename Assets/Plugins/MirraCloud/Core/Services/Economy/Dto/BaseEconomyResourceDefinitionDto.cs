using System;
using JetBrains.Annotations;

namespace MirraCloud.Core.Economy
{
    [Serializable]
    public abstract class BaseEconomyResourceDefinitionDto
    {
        public string name;
        public string id;
        public IconDto icon;
        [CanBeNull] public string parentCategoryId;
        public EntityConfigDto config;
    }

    public class EntityConfigDto
    {
        
    }
    
    
}