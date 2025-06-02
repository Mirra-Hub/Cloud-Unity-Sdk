using System;

namespace MirraCloud.Core.Economy
{
    [Serializable]
    public abstract class BaseEconomyResourceDefinitionDto
    {
        public string name;
        public string id;
        public EconomyIconDto icon;
    }
}