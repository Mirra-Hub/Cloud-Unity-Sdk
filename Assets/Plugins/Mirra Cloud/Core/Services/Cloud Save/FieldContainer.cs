using System.Collections.Generic;

namespace MirraCloud.Core.CloudSave
{
    public class FieldContainer<T>
    {
        private readonly Dictionary<string, T> _fields = new Dictionary<string, T>();

        public IReadOnlyDictionary<string, T> Fields => _fields;
        
        public T GetValue(string key, T defaultValue = default)
        {
            if (_fields.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}