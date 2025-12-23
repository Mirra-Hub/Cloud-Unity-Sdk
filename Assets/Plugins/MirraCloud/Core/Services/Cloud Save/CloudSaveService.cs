using MirraCloud.Core.CloudSave.Responses;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.General.AsyncOperations;

namespace MirraCloud.Core.CloudSave
{
    public class CloudSaveService 
    {
        private const string ControllerApi = "/cloud-save/v1";
        
        private readonly IJsonService _jsonService;
        private readonly Logger.ILogger _logger;
        private readonly RestApiClient _restApi;
        private readonly Configuration _configuration;
        
        public PlayerData PlayerData { get; private set; }

        public CloudSaveService(Configuration configuration, Logger.ILogger logger, IJsonService jsonService, RestApiClient restApi)
        {
            _configuration = configuration;
            _restApi = restApi;
            _logger = logger;
            _jsonService = jsonService;
        }

        public AsyncOperation<RestApiResult<PlayerDataResponse>> LoadAsync()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/load";
            
            var request = _restApi.GetAsync<PlayerDataResponse>(route);
            
            request.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess && completed.Result.Data != null)
                {
                    PlayerData = new PlayerData(completed.Result.Data);
                    
                    foreach (var field in PlayerData.Fields)
                    {
                        _logger.Log($"key {field.Key} value {field.CurrentValue}");
                    }
                }
            };

            return request;
        }

        public AsyncOperation<RestApiResult<PlayerDataResponse>> SaveAsync(UpdateDataContainer data)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/update";
            
            var request = _restApi.PostAsync<PlayerDataResponse>(route, data);

            request.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess && completed.Result.Data != null)
                {
                    PlayerData = new PlayerData(completed.Result.Data);
                    
                    foreach (var field in PlayerData.Fields)
                    {
                        _logger.Log($"key {field.Key} value {field.CurrentValue}");
                    }
                }
            };
            
            return request;
        }
    }
}
