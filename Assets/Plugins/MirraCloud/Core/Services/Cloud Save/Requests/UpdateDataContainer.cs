using System;
using System.Collections.Generic;
using UnityEngine;
using Voorhees;

namespace MirraCloud.Core.CloudSave
{
    [Serializable]
    public class UpdateDataContainer
    {
        [SerializeField] private List<UpdateDataFieldValue> Fields = new List<UpdateDataFieldValue>();
        
        public void AddString(string key, string value)
        {
            Fields.Add(new UpdateDataFieldValue(key, value));
        }

        public void AddList<T>(string key, List<T> value)
        {
            var json = JsonMapper.ToJson(value);
            Fields.Add(new UpdateDataFieldValue(key, json));
        }
    }
    
    [Serializable]
    public struct UpdateDataFieldValue
    {
        public string Key;
        public string Value;

        public UpdateDataFieldValue(string key, string currentValue)
        {
            Key = key;
            Value = currentValue;
        }
    }
}