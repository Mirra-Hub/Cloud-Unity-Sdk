using MirraCloud.Core.RemoteConfig.Responses;
using UnityEngine;
using ILogger = MirraCloud.Core.Logger.ILogger;

namespace MirraCloud.Core.RemoteConfig
{
    public class RemoteConfigService
    {
        private readonly RestApiClient _restApi;
        private readonly Configuration _configuration;
        private readonly ILogger _logger;

        public RemoteConfig Config { get; private set; }

        private const string SERVICE_ROUTE = "/remote-config/v1";
        
        public RemoteConfigService(RestApiClient restApi, Configuration configuration, ILogger logger)
        {
            _logger = logger;
            _restApi = restApi;
            _configuration = configuration;
        }

        public IRestApiOperation LoadConfigAsync()
        {
            var request = _restApi.Get($"{SERVICE_ROUTE}/{_configuration.ProjectId}/config");
            
            request.UseCompletedCallback(response =>
            {
                if (request.IsSuccess)
                {
                    Debug.Log(request.DownloadHandler.text);
                    
                    var configResponse = response.GetData<FetchRemoteConfigResponse>();

                    
                    Config = new RemoteConfig(configResponse.configs[0]);
                }
            });
            
            return request;
        }
    }
}