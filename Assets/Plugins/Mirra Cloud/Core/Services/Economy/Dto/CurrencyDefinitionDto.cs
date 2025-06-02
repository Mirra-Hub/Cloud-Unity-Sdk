using System;

namespace MirraCloud.Core.Economy
{
    [Serializable]
    public class CurrencyDefinitionDto : BaseEconomyResourceDefinitionDto
    {
        public int minValue;
        public int maxValue;
    }
}