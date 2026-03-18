using System.Collections.Generic;
using MirraCloud.Core.AssetsStorage;
using MirraCloud.Core.Auth;
using MirraCloud.Core.Chats;
using MirraCloud.Core.CloudSave;
using MirraCloud.Core.CloudCode;
using MirraCloud.Core.Economy;
using MirraCloud.Core.Entities;
using MirraCloud.Core.Friends;
using MirraCloud.Core.Groups;
using MirraCloud.Core.Leaderboard;
using MirraCloud.Core.Logger;
using MirraCloud.Core.RemoteConfig;
using MirraCloud.Core.Storage;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.General.LifeCycle;
using Plugins.MirraCloud.Core.Services.Analytics;
using Plugins.MirraCloud.Core.Services.Deployment;
using Plugins.MirraCloud.Core.Services.PlayerAccount;
using Plugins.MirraCloud.Core.Services.Segments;
using Plugins.MirraCloud.Core.Services.Tournaments;

namespace MirraCloud.Core
{
    public static class MirraCloudSDK
    {
        private static AnalyticsTracker _analyticsTracker;

        public static AuthenticationService Authentication { get; private set; }
        public static PlayerAccountService PlayerAccount { get; private set; }
        public static FriendsService Friends { get; private set; }
        public static ChatsService Chats { get; private set; }
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
        public static GroupsService Groups { get; private set; }
        
        public static bool IsInitialized { get; private set; }

        private static List<ICloudSdkDisposable> _disposables = new List<ICloudSdkDisposable>();
        private static List<ICloudSdkInitializable> _initializables = new List<ICloudSdkInitializable>();

        private static void RegisterInitialize(ICloudSdkInitializable initializable)
        {
            _initializables.Add(initializable);
        }
        
        private static void RegisterDispose(ICloudSdkDisposable disposable)
        {
            _disposables.Add(disposable);
        }
        
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
            Groups = new GroupsService(configuration, logger, restApiClient);

            Chats = new ChatsService(configuration, logger, restApiClient, jsonService);
            RegisterInitialize(Chats);
            RegisterDispose(Chats);
            
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

            _analyticsTracker = AnalyticsTracker.CreateInstance();
            Analytics.SetTracker(_analyticsTracker);
            Authentication.OnLogin += _ =>
            {
                Analytics.SendSessionStartedAsync();
                _analyticsTracker.StartTracking(Analytics);
            };
            
            foreach (var cloudSdkInitializable in _initializables)
            {
                cloudSdkInitializable.CloudSdkInitialize();
            }
            
            IsInitialized = true;
            
        }

        public static void Dispose()
        {
            foreach (var cloudSdkDisposable in _disposables)
            {
                cloudSdkDisposable.CloudSdkDispose();
            }

            PlayerAccount.Dispose();
        }
    }
}
