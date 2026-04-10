using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MirraCloud
{
    [CreateAssetMenu(menuName = "Mirra Cloud/Create Configuration", fileName = "Configuration", order = 0)]
    public class Configuration : ScriptableObject
    {
        [Header("General")]
        public string ProjectId;
        public string BranchId;
        public string Token;
        public string AnalyticsPlatformId;

        [Header("Editor")]
        [SerializeField] private string _editorApiUrl;

        [Header("Debug")]
        [SerializeField] private int _connectionIndex = -1;
        [SerializeField] private ConnectionSettings[] _connections;

        private const string RESOUCRES_PATH = "Configuration";

        public string Url { get; private set; }
        public string EditorApiUrl => _editorApiUrl;

        private void Initialize()
        {
            ConnectionSettings connectionSettings = GetConnectionSettings();

            Url = connectionSettings.Url;
        }
        
        public string CreateUrlApi(string router)
        {
            string path = Url + router;
            
            return path;
        }

        public static Configuration Load()
        {
            Configuration configuration = Resources.Load<Configuration>(RESOUCRES_PATH);
            configuration.Initialize();
            
            return configuration;
        }

        public ConnectionSettings GetConnectionSettings()
        {
            return _connections[_connectionIndex];
        }
    }

    [Serializable]
    public class ConnectionSettings
    {
        public string Url;
    }
}