using System;
using System.Collections.Generic;

namespace MirraCloud.Core.Economy
{
    [Serializable]
    public class EconomyConfigDto
    {
        public List<CurrencyDefinitionDto> currencies;
        public List<ItemDefinitionDto> items;
        public List<TradeDefinitionDto> virtualPurchases;
    }
}