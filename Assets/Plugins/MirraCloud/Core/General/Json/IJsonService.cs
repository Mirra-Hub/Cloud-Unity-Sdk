using System.Collections.Generic;

namespace MirraCloud
{
    public interface IJsonService
    {
        string ToJson(object val, bool prettyPrint = false);
        T FromJson<T>(string json);
    }
}