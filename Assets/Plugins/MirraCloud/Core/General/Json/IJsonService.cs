using System.Collections.Generic;

namespace MirraCloud
{
    public interface IJsonService
    {
        string ToJson(object obj);
        string ListToJson<T>(List<T> list);
        string ArrayToJson<T>(T[] arr);
        T FromJson<T>(string json);
        List<T> ListFromJson<T>(string json);
        T[] ArrayFromJson<T>(string json);
        bool TryFromJson<T>(string json, out T result, T defaultValue = default);
    }
}