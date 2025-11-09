using MirraCloud.Core.CloudSave.Responses;
using MirraCloud.Json;

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

        public IRestApiOperation LoadAsync()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/load";
            
            var request = _restApi.Get(route);
            
            request.UseCompletedCallback(response =>
            {
                if (response.IsSuccess)
                {
                    _logger.Log(request.DownloadHandler.text);
                    
                    var playerDataResponse = request.GetData<PlayerDataResponse>();

                    PlayerData = new PlayerData(playerDataResponse);
                    
                    foreach (var field in PlayerData.Fields)
                    {
                        _logger.Log($"key {field.Key} value {field.CurrentValue}");
                    }
                }
            });

            return request;
        }

        public IRestApiOperation SaveAsync(UpdateDataContainer data)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/update";
            
            var request = _restApi.Post(route, data);

            request.UseCompletedCallback(response =>
            {
                if (response.IsSuccess)
                {
                    _logger.Log(request.DownloadHandler.text);
                    
                    var playerDataResponse = request.GetData<PlayerDataResponse>();

                    PlayerData = new PlayerData(playerDataResponse);
                    
                    foreach (var field in PlayerData.Fields)
                    {
                        _logger.Log($"key {field.Key} value {field.CurrentValue}");
                    }
                }
            });
            
            return request;
        }
    }
}
