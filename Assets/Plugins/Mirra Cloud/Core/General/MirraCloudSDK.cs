using MirraCloud.Core.AssetsStorage;
using MirraCloud.Core.Auth;
using MirraCloud.Core.CloudSave;
using MirraCloud.Core.Economy;
using MirraCloud.Core.Leaderboard;
using MirraCloud.Core.Logger;
using MirraCloud.Core.RemoteConfig;
using MirraCloud.Core.Storage;

namespace MirraCloud.Core
{
    public static class MirraCloudSDK
    {
        public static AuthenticationService Authentication { get; private set; }
        public static EconomyService Economy { get; private set; }
        public static CloudSaveService CloudSave { get; private set; }
        public static LeaderboardService Leaderboard { get; private set; }
        public static RemoteConfigService RemoteConfig { get; private set; }
        public static AssetsStorageService AssetsStorage { get; private set; }
        
        public static bool IsInitialized { get; private set; }

        public static void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            CoroutineRunner coroutineRunner = CoroutineRunner.CreateInstance();
            
            Configuration configuration = Configuration.Load();
            ILogger logger = new Core.Logger.Logger();
            IJsonService jsonService = new JsonService();

            RestApiClientOptions restApiClientOptions = new RestApiClientOptions()
            {
                BaseUrl = configuration.Url,
            };

            RestApiClient restApiClient = new RestApiClient(restApiClientOptions, coroutineRunner, jsonService, logger);
            
            IStorage storage = new PrefsStorage();
            
            Authentication = new AuthenticationService(configuration, logger, storage, restApiClient);
            Economy = new EconomyService(configuration, logger, restApiClient);
            CloudSave = new CloudSaveService(configuration, logger, jsonService, restApiClient);
            Leaderboard = new LeaderboardService(configuration, logger, jsonService, restApiClient);
            RemoteConfig = new RemoteConfigService(restApiClient, configuration, logger);
            AssetsStorage = new AssetsStorageService(configuration, restApiClient, logger);
            
            IsInitialized = true;
        }
    }
}
