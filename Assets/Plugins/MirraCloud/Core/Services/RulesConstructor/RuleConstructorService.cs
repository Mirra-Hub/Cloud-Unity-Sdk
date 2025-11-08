using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.Services.RulesConstructor.Dto;
using UnityEngine;
using ILogger = MirraCloud.Core.Logger.ILogger;

namespace Plugins.MirraCloud.Core.Services.RulesConstructor
{
    public class RuleConstructorService
    {
        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private readonly RestApiClient _restApi;
        private readonly IJsonService _jsonService;

        private const string ControllerApi =  "/rules-constructor";
        
        public RuleConstructorService(Configuration configuration, ILogger logger, RestApiClient restApiClient, IJsonService jsonService)
        {
            _configuration = configuration;
            _logger = logger;
            _restApi = restApiClient;
            _jsonService = jsonService;
        }
        
        public IRestApiOperation LoadConfigAsync()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/env/{_configuration.BranchId}/rules";
            
            var response = _restApi.Get(route);

            response.UseCompletedCallback(result =>
            {
                _logger.Log(result.DownloadHandler.text);
                
                var rulesTreeDto = _jsonService.FromJson<RulesTreeDto[]>(response.DownloadHandler.text);
                
                
                
                Debug.Log(rulesTreeDto);
                Debug.Log(_jsonService.ToJson(rulesTreeDto));
            });
            
            return response;
        }
    }
}