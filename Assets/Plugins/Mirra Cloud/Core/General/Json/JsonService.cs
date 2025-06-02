using System;
using System.Collections.Generic;
using MirraCloud.Core.CloudSave;
using UnityEngine;

namespace MirraCloud
{
    public class JsonService : IJsonService
    {
        public string ToJson(object obj)
        {
            return JsonUtility.ToJson(obj);
        }
        
        public string ListToJson<T>(List<T> list)
        {
            ListJsonWrapper<T> wrapper = new ListJsonWrapper<T>(list);
            
            return JsonUtility.ToJson(wrapper);
        }
        
        public string ArrayToJson<T>(T[] arr)
        {
            ArrayJsonWrapper<T> wrapper = new ArrayJsonWrapper<T>(arr);
            
            return JsonUtility.ToJson(wrapper);
        }
        
        public T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
        
        public bool TryFromJson<T>(string json, out T result, T defaultValue = default)
        {
            try
            {
                result = JsonUtility.FromJson<T>(json);
                return true;
            }
            catch (Exception e)
            {
                result = defaultValue;
                return false;
            }
        }
        
        public List<T> ListFromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new List<T>();
            }
            
            ListJsonWrapper<T> wrapper = JsonUtility.FromJson<ListJsonWrapper<T>>(json);

            return wrapper.Values;
        }
        
        public T[] ArrayFromJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new T[0];
            }
            
            ArrayJsonWrapper<T> wrapper = JsonUtility.FromJson<ArrayJsonWrapper<T>>(json);

            return wrapper.Values;
        }
    }
}