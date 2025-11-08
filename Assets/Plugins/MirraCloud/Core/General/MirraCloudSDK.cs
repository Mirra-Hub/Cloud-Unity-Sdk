using MirraCloud.Core.AssetsStorage;
using MirraCloud.Core.Auth;
using MirraCloud.Core.CloudSave;
using MirraCloud.Core.Economy;
using MirraCloud.Core.Leaderboard;
using MirraCloud.Core.Logger;
using MirraCloud.Core.RemoteConfig;
using MirraCloud.Core.Storage;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.Services.PlayerAccount;
using Plugins.MirraCloud.Core.Services.RulesConstructor;
using Plugins.MirraCloud.Core.Services.Segments;

namespace MirraCloud.Core
{
    public static class MirraCloudSDK 
    {
        public static AuthenticationService Authentication { get; private set; }
        public static PlayerAccountService PlayerAccount { get; private set; }
        public static EconomyService Economy { get; private set; }
        public static CloudSaveService CloudSave { get; private set; }
        public static LeaderboardService Leaderboard { get; private set; }
        public static RemoteConfigService RemoteConfig { get; private set; }
        public static AssetsStorageService AssetsStorage { get; private set; }
        public static RuleConstructorService RuleConstructor { get; private set; }
        public static SegmentService Segments { get; private set; }
        
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
            IJsonService jsonService = new JsonService()
                .RegisterImporter(new RuleConstructorJsonMapper());

            RestApiClientOptions restApiClientOptions = new RestApiClientOptions()
            {
                BaseUrl = configuration.Url,
            };

            RestApiClient restApiClient = new RestApiClient(restApiClientOptions, coroutineRunner, jsonService, logger);
            
            IStorage storage = new PrefsStorage();
            
            Authentication = new AuthenticationService(configuration, logger, storage, restApiClient);
            PlayerAccount = new PlayerAccountService(Authentication, restApiClient, configuration, logger);
            Economy = new EconomyService(configuration, logger, restApiClient);
            CloudSave = new CloudSaveService(configuration, logger, jsonService, restApiClient);
            Leaderboard = new LeaderboardService(configuration, logger, jsonService, restApiClient);
            RemoteConfig = new RemoteConfigService(restApiClient, configuration, logger);
            AssetsStorage = new AssetsStorageService(configuration, restApiClient, logger);

            RuleConstructor = new RuleConstructorService(configuration, logger, restApiClient, jsonService);
            Segments = new SegmentService(configuration, logger, restApiClient, jsonService);
            
            IsInitialized = true;
        }

        public static void Dispose()
        {
            PlayerAccount.Dispose();
        }
    }
}
