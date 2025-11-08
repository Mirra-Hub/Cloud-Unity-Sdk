using System;
using System.Globalization;
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
            var capturedJson = CaptureCurrentJson(reader);
            var jsonValue = JsonMapper.FromJson<JsonValue>(capturedJson);
            var sourceType = ParseSourceType(jsonValue);
            var targetType = ResolveNodeType(sourceType);
            return (BaseNodeDto)_mapper.Read(targetType, new JsonTokenReader(new StringReader(capturedJson)));
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

        private string CaptureCurrentJson(JsonTokenReader reader)
        {
            var stringBuilder = new StringBuilder();
            WriteValue(stringBuilder, reader);
            return stringBuilder.ToString();
        }

        private void WriteValue(StringBuilder builder, JsonTokenReader reader)
        {
            switch (reader.NextToken)
            {
                case JsonToken.Null:
                    builder.Append("null");
                    reader.SkipToken(JsonToken.Null);
                    return;
                case JsonToken.True:
                    builder.Append("true");
                    reader.SkipToken(JsonToken.True);
                    return;
                case JsonToken.False:
                    builder.Append("false");
                    reader.SkipToken(JsonToken.False);
                    return;
                case JsonToken.Number:
                    builder.Append(reader.ConsumeNumber().ToString(CultureInfo.InvariantCulture));
                    return;
                case JsonToken.String:
                    builder.Append('"');
                    builder.Append(reader.ConsumeString());
                    builder.Append('"');
                    return;
                case JsonToken.ObjectStart:
                    WriteObject(builder, reader);
                    return;
                case JsonToken.ArrayStart:
                    WriteArray(builder, reader);
                    return;
                default:
                    throw new InvalidJsonException($"{reader.LineColString} Unexpected token {reader.NextToken}");
            }
        }

        private void WriteObject(StringBuilder builder, JsonTokenReader reader)
        {
            builder.Append('{');
            reader.SkipToken(JsonToken.ObjectStart);

            bool first = true;
            while (reader.NextToken != JsonToken.ObjectEnd)
            {
                if (!first)
                {
                    builder.Append(',');
                }
                first = false;

                var propertyName = reader.ConsumeString();
                builder.Append('"').Append(propertyName).Append('"').Append(':');
                reader.SkipToken(JsonToken.KeyValueSeparator);

                WriteValue(builder, reader);

                if (reader.NextToken == JsonToken.Separator)
                {
                    reader.SkipToken(JsonToken.Separator);
                }
            }

            reader.SkipToken(JsonToken.ObjectEnd);
            builder.Append('}');
        }

        private void WriteArray(StringBuilder builder, JsonTokenReader reader)
        {
            builder.Append('[');
            reader.SkipToken(JsonToken.ArrayStart);

            bool first = true;
            while (reader.NextToken != JsonToken.ArrayEnd)
            {
                if (!first)
                {
                    builder.Append(',');
                }
                first = false;

                WriteValue(builder, reader);

                if (reader.NextToken == JsonToken.Separator)
                {
                    reader.SkipToken(JsonToken.Separator);
                }
            }

            reader.SkipToken(JsonToken.ArrayEnd);
            builder.Append(']');
        }
    }
}