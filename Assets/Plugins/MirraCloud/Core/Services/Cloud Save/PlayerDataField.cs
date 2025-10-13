using System;

namespace MirraCloud.Core.CloudSave
{
    public struct PlayerDataField
    {
        public string Key;
        public string CurrentValue;
        public CloudSaveFieldType FieldType;

        public PlayerDataField(string key, string currentValue, CloudSaveFieldType fieldType)
        {
            Key = key;
            CurrentValue = currentValue;
            FieldType = fieldType;
        }
    }
}