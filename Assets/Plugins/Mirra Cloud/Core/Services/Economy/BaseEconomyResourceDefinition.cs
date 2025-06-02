namespace MirraCloud.Core.Economy
{
    public abstract class BaseEconomyResourceDefinition
    {
        public readonly string Name;
        public readonly string ResourceId;
        public readonly IconDefinition Icon;

        protected BaseEconomyResourceDefinition(BaseEconomyResourceDefinitionDto dto)
        {
            Name = dto.name;
            ResourceId = dto.id;
            Icon = new IconDefinition(dto.icon);
        }
    }

    public readonly struct IconDefinition
    {
        public readonly string Value;
        public bool HasIcon => string.IsNullOrEmpty(Value) == false;

        public IconDefinition(EconomyIconDto dto)
        {
            Value = dto.value;
        }

    }

    public class ItemEconomyDefinition : BaseEconomyResourceDefinition
    {
        public ItemEconomyDefinition(ItemDefinitionDto dto) : base(dto)
        {
        }
    }
    
    public class CurrencyEconomyDefinition : BaseEconomyResourceDefinition
    {
        public readonly int MinValue;
        public readonly int MaxValue;
        
        public CurrencyEconomyDefinition(CurrencyDefinitionDto dto) : base(dto)
        {
            MinValue = dto.minValue;
            MaxValue = dto.maxValue;
        }
    }
    
    public class TradeEconomyDefinition : BaseEconomyResourceDefinition
    {
        public TradeEconomyDefinition(TradeDefinitionDto dto) : base(dto)
        {
        }
    }
}