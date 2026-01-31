using MirraCloud;
using MirraCloud.Core.AssetsStorage;
using MirraCloud.Core.Auth;
using MirraCloud.Core.CloudSave;
using MirraCloud.Core.CloudCode;
using MirraCloud.Core.Economy;
using MirraCloud.Core.Entities;
using MirraCloud.Core.Friends;
using MirraCloud.Core.Leaderboard;
using MirraCloud.Core.Logger;
using MirraCloud.Core.RemoteConfig;
using MirraCloud.Core.Storage;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.Services.Analytics;
using Plugins.MirraCloud.Core.Services.Deployment;
using Plugins.MirraCloud.Core.Services.PlayerAccount;
using Plugins.MirraCloud.Core.Services.Segments;
using Plugins.MirraCloud.Core.Services.Tournaments;

namespace MirraCloud.Core
{
    public static class MirraCloudSDK 
    {
        public static AuthenticationService Authentication { get; private set; }
        public static PlayerAccountService PlayerAccount { get; private set; }
        public static FriendsService Friends { get; private set; }
        public static EconomyService Economy { get; private set; }
        public static EntitiesService Entities { get; private set; }
        public static CloudSaveService CloudSave { get; private set; }
        public static LeaderboardService Leaderboard { get; private set; }
        public static TournamentsService Tournaments { get; private set; }
        public static RemoteConfigService RemoteConfig { get; private set; }
        public static AssetsStorageService AssetsStorage { get; private set; }
        public static CloudCodeService CloudCode { get; private set; }
        public static SegmentService Segments { get; private set; }
        public static AnalyticsService Analytics { get; private set; }
        public static DeploymentService Deployment { get; private set; }
        
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
            PlayerAccount = new PlayerAccountService(Authentication, restApiClient, configuration, logger);
            Friends = new FriendsService(configuration, logger, restApiClient);
            Economy = new EconomyService(configuration, logger, restApiClient);
            Entities = new EntitiesService(configuration, logger, restApiClient);
            CloudSave = new CloudSaveService(configuration, logger, jsonService, restApiClient);
            Leaderboard = new LeaderboardService(configuration, PlayerAccount, logger, jsonService, restApiClient);
            Tournaments = new TournamentsService(configuration, restApiClient);
            RemoteConfig = new RemoteConfigService(restApiClient, configuration, logger);
            AssetsStorage = new AssetsStorageService(configuration, restApiClient, logger);
            Analytics = new AnalyticsService(configuration, logger, restApiClient);
            Deployment = new DeploymentService(configuration, logger, restApiClient);
            CloudCode = new CloudCodeService(configuration, logger, restApiClient);

            Segments = new SegmentService(configuration, logger, restApiClient);
            
            IsInitialized = true;
        }

        public static void Dispose()
        {
            PlayerAccount.Dispose();
        }
    }
}
