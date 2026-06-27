using System.Globalization;
using MirraCloud.Json;

namespace MirraCloud.Example.Showcase
{
    /// <summary>Small formatting helpers shared by the data/config views (truncation + safe
    /// JsonValue stringification — JsonValue has no usable ToString()).</summary>
    public static class Fmt
    {
        public static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s ?? string.Empty;
            }
            return s.Length <= max ? s : s.Substring(0, max) + "…";
        }

        /// <summary>Stringify a dynamic JsonValue for table display (branch on Type; containers show a count).</summary>
        public static string Json(JsonValue v)
        {
            if (v == null)
            {
                return "null";
            }
            switch (v.Type)
            {
                case JsonValueType.Null: return "null";
                case JsonValueType.String: return (string)v;
                case JsonValueType.Boolean: return ((bool)v).ToString();
                case JsonValueType.Int: return ((int)v).ToString();
                case JsonValueType.Double: return ((double)v).ToString(CultureInfo.InvariantCulture);
                case JsonValueType.Object: return "{ " + v.Count + " keys }";
                case JsonValueType.Array: return "[ " + v.Count + " items ]";
                default: return string.Empty;
            }
        }
    }
}
