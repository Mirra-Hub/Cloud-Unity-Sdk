using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MirraCloud.Core.Economy;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Converters;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace Plugins.MirraCloud.Core.Services.EntityConfig
{
    public class EntityConfig
    {
        
    }
    
    [JsonConverter(typeof(NodeDtoConverter))]
    public abstract class NodeDto
    {
 
        public string Id { get; set; } = null!;

        public string? Key { get; set; }

        public JsonType Type { get; set; }
    }

    public class ImageNodeDto : NodeDto
    {
        [JsonProperty("value")]
        public IconDto Value { get; set; }
    }
    
    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum JsonType
    {
        Object,
        Array,
        String,
        Number,
        Boolean,
        Null,
        Image,
    }
    
    public class CollectionNodeDto : NodeDto
    {
        public List<NodeDto> Value { get; set; } = new();
    }
    
    public class PrimitiveNodeDto : NodeDto
    {
        [CanBeNull] public object Value { get; set; }
    }
    
    public class NodeDtoConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
            => typeof(NodeDto).IsAssignableFrom(objectType);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            try
            {
                // Загружаем JObject один раз
                var jo = JObject.Load(reader);

                // Логируем весь полученный JSON
                Console.WriteLine("[NodeDtoConverter] Incoming JSON:\n" + jo.ToString(Formatting.Indented));

                // Дискриминатор
                var typeProp = jo["type"]?.Value<string>();
                Console.WriteLine("[NodeDtoConverter] Found typeProp: " + (typeProp ?? "<null>"));

                if (typeProp == null)
                    throw new JsonSerializationException("Missing type discriminator");

                if (!Enum.TryParse(typeProp, true, out JsonType jt))
                    throw new JsonSerializationException($"Unknown JsonType: '{typeProp}'");

                Console.WriteLine("[NodeDtoConverter] Parsed JsonType: " + jt);

                // Общие поля
                var id  = jo["id"]?.Value<string>() 
                          ?? throw new JsonSerializationException("Missing id");
                var key = jo["key"]?.Value<string>(); 
                Console.WriteLine($"[NodeDtoConverter] Id = {id}, Key = {(key ?? "<null>")}");

                // 1) Новый тип image
                if (jt == JsonType.Image)
                {
                    Console.WriteLine("[NodeDtoConverter] Deserializing as ImageNodeDto");
                    var img = new ImageNodeDto { Id = id, Key = key, Type = jt };
                    if (jo["value"] is JObject iv && iv["value"]?.Type == JTokenType.String)
                    {
                        img.Value = new IconDto {
                            value = iv["value"]!.Value<string>()!
                        };
                        Console.WriteLine("[NodeDtoConverter] Image value = " + img.Value.value);
                    }
                    return img;
                }

                // 2) object/array
                if (jt == JsonType.Object || jt == JsonType.Array)
                {
                    Console.WriteLine($"[NodeDtoConverter] Deserializing as CollectionNodeDto ({jt})");
                    var coll = new CollectionNodeDto { Id = id, Key = key, Type = jt };
                    if (jo["value"] is JArray arr)
                    {
                        Console.WriteLine($"[NodeDtoConverter] Collection has {arr.Count} children");
                        foreach (var childToken in arr)
                        {
                            // Рекурсивно десериализуем каждую ноду
                            var child = childToken.ToObject<NodeDto>(serializer)
                                        ?? throw new JsonSerializationException("Failed to deserialize child node");
                            coll.Value.Add(child);
                        }
                    }
                    return coll;
                }

                // 3) примитивы
                Console.WriteLine("[NodeDtoConverter] Deserializing as PrimitiveNodeDto");
                var prim = new PrimitiveNodeDto { Id = id, Key = key, Type = jt };
                if (jo.TryGetValue("value", out var valToken))
                {
                    prim.Value = valToken.Type switch
                    {
                        JTokenType.Null    => null,
                        JTokenType.Boolean => valToken.Value<bool>(),
                        JTokenType.Integer => valToken.Value<long>(),
                        JTokenType.Float   => valToken.Value<double>(),
                        JTokenType.String  => valToken.Value<string>(),
                        _ => valToken.ToObject<object>(serializer)
                    };
                    Console.WriteLine("[NodeDtoConverter] Primitive value = " + prim.Value);
                }
                return prim;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[NodeDtoConverter] Exception: " + ex);
                throw;
            }
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            try
            {
                if (value is not NodeDto node)
                {
                    writer.WriteNull();
                    return;
                }

                Console.WriteLine($"[NodeDtoConverter] Serializing node id={node.Id}, type={node.Type}");

                // 1) image
                if (node is ImageNodeDto img)
                {
                    var jImg = new JObject {
                        ["id"]   = img.Id,
                        ["type"] = JToken.FromObject(img.Type, serializer)
                    };
                    if (img.Key is not null) jImg["key"] = img.Key;
                    jImg["value"] = new JObject { ["value"] = img.Value.value };
                    jImg.WriteTo(writer);
                    return;
                }

                // 2) object/array или примитив
                var jo = new JObject {
                    ["id"]   = node.Id,
                    ["type"] = JToken.FromObject(node.Type, serializer)
                };
                if (node.Key is not null) jo["key"] = node.Key;

                switch (node)
                {
                    case CollectionNodeDto coll:
                        var arr = new JArray();
                        foreach (var child in coll.Value)
                            arr.Add(JToken.FromObject(child, serializer));
                        jo["value"] = arr;
                        break;

                    case PrimitiveNodeDto prim:
                        jo["value"] = prim.Value is null
                            ? JValue.CreateNull()
                            : JToken.FromObject(prim.Value, serializer);
                        break;
                }

                jo.WriteTo(writer);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[NodeDtoConverter] WriteJson Exception: " + ex);
                throw;
            }
        }
    }
}