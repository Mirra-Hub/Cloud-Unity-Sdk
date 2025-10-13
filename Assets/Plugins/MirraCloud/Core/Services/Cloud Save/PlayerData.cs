using System.Collections.Generic;
using MirraCloud.Core.CloudSave.Responses;

namespace MirraCloud.Core.CloudSave
{
    public class PlayerData
    {
        private readonly List<PlayerDataField> _fields = new List<PlayerDataField>();

        public IReadOnlyList<PlayerDataField> Fields => _fields;

        private readonly Dictionary<string, string> _stringFields = new Dictionary<string, string>();
        private readonly Dictionary<string, bool> _boolFields = new Dictionary<string, bool>();
        private readonly Dictionary<string, float> _floatFields = new Dictionary<string, float>();
        private readonly Dictionary<string, int> _intFields = new Dictionary<string, int>();

        public PlayerData(PlayerDataResponse playerDataResponse)
        {
            foreach (var field in playerDataResponse.data)
            {
                if (string.IsNullOrEmpty(field.key))
                {
                    continue;
                }
                
                _fields.Add(new PlayerDataField(field.key, field.value, field.fieldType));

                switch (field.fieldType)
                {
                    case CloudSaveFieldType.String:
                    {
                        _stringFields.Add(field.key, field.value);
                        break;
                    }
                    case CloudSaveFieldType.Boolean:
                    {
                        if (bool.TryParse(field.value, out var value))
                        {
                            _boolFields.Add(field.key, value);
                        }
                        break;
                    }
                    case CloudSaveFieldType.Float:
                    {
                        if (float.TryParse(field.value, out var value))
                        {
                            _floatFields.Add(field.key, value);
                        }
                        break;
                    }
                    case CloudSaveFieldType.Int:
                    {
                        if (int.TryParse(field.value, out var value))
                        {
                            _intFields.Add(field.key, value);
                        }
                        break;
                    }
                }
            }
        }
        
        public string GetString(string key, string defaultValue = "")
        {
            if (_stringFields.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
        
        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_boolFields.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
        
        public int GetInt(string key, int defaultValue = 0)
        {
            if (_intFields.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
        
        public float GetFloat(string key, float defaultValue = 0)
        {
            if (_floatFields.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }
    }
}