using UnityEngine;

namespace MirraCloud
{
    [CreateAssetMenu(menuName = "Mirra Cloud/Create Configuration", fileName = "Configuration", order = 0)]
    public class Configuration : ScriptableObject
    {
        private const string PROD_SDK_URL = "https://sdk.mirrahub.com/api/cloud/sdk";
        private const string PROD_EDITOR_URL = "https://mirrahub.com";
        private const string RESOURCES_PATH = "Configuration";

        [Header("General")]
        public string ProjectId;
        public string BranchId;
        public string Token;
        public string AnalyticsPlatformId;

        public string Url { get; private set; }
        public string EditorApiUrl { get; private set; }

        private void Initialize()
        {
            Url = PROD_SDK_URL;
            EditorApiUrl = PROD_EDITOR_URL;

            var devSettings = DeveloperSettings.TryLoad();
            var profile = devSettings != null ? devSettings.ActiveProfile : null;
            if (profile != null)
            {
                if (string.IsNullOrEmpty(profile.SdkUrl) == false)
                {
                    Url = profile.SdkUrl;
                }

                if (string.IsNullOrEmpty(profile.EditorUrl) == false)
                {
                    EditorApiUrl = profile.EditorUrl;
                }
            }
        }


        public static Configuration Load()
        {
            Configuration configuration = Resources.Load<Configuration>(RESOURCES_PATH);
            configuration.Initialize();
            return configuration;
        }
    }
}