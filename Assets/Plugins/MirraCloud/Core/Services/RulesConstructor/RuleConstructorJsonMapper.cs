using System;
using System.IO;
using MirraCloud;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Dto;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;
using MirraCloud.Json;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor
{
    public class RuleConstructorJsonMapper : IJsonImporter<BaseNodeDto>
    {
        public BaseNodeDto ImportJson(JsonMapper mapper, JsonTokenReader reader)
        {
            var node = mapper.ReadJsonNode(reader);
            var sourceType = ParseSourceType(node);
            var targetType = ResolveNodeType(sourceType);
            var json = JsonMapper.ToJson(node);
            return (BaseNodeDto)mapper.Read(targetType, new JsonTokenReader(new StringReader(json)));
        }

        private SourceType ParseSourceType(JsonValue value)
        {
            if (!value.ContainsKey("sourceType"))
            {
                throw new InvalidJsonException("sourceType field is missing");
            }

            var rawSourceType = value["sourceType"];

            return rawSourceType.Type switch
            {
                JsonValueType.Int => (SourceType)(int)rawSourceType,
                JsonValueType.Double => (SourceType)(int)(double)rawSourceType,
                JsonValueType.String => Enum.Parse<SourceType>((string)rawSourceType, true),
                _ => throw new InvalidJsonException("sourceType field has unsupported type")
            };
        }

        private Type ResolveNodeType(SourceType sourceType)
        {
            return sourceType switch
            {
                SourceType.And => typeof(AndNodeDto),
                SourceType.Or => typeof(OrNodeDto),
                SourceType.ActiveDays => typeof(DayActiveNodeDto),
                SourceType.InventoryItem => typeof(InventoryItemNodeDto),
                _ => throw new InvalidJsonException($"Unsupported sourceType value {sourceType}")
            };
        }
    }
}