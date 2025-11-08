using System;
using System.IO;
using System.Text;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Dto;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Enums;
using Voorhees;

namespace MirraCloud
{
    public class JsonService : IJsonService
    {
        private readonly JsonMapper _mapper;

        public JsonService()
        {
            _mapper = new JsonMapper();

            CreateMapper();
        }
        
        public string ToJson(object val, bool prettyPrint = false)
        {
            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder)) {
                _mapper.Write(val, new JsonTokenWriter(stringWriter, prettyPrint));
            }
            return stringBuilder.ToString();
        }

        public T FromJson<T>(string json)
        {
            return _mapper.Read<T>(new JsonTokenReader(new StringReader(json)));
        }
        
        private void CreateMapper()
        {
            _mapper.RegisterImporter<BaseNodeDto>(ParseBaseNode);
        }

        private BaseNodeDto ParseBaseNode(JsonTokenReader reader)
        {
            var node = _mapper.ReadJsonNode(reader);
            var sourceType = ParseSourceType(node);
            var targetType = ResolveNodeType(sourceType);
            var json = JsonMapper.ToJson(node);
            return (BaseNodeDto)_mapper.Read(targetType, new JsonTokenReader(new StringReader(json)));
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