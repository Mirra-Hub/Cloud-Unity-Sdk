using UnityEngine;

namespace MirraCloud.Core.Storage
{
    public class PrefsStorage : IStorage
    {
        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public string GetString(string key)
        {
            return PlayerPrefs.GetString(key);
        }

        public void SaveString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }
    }
}