using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Core.Logger;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.Services.Deployment.Dto;

namespace Plugins.MirraCloud.Core.Services.Deployment
{
    public class DeploymentService
    {
        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private readonly RestApiClient _restApi;
        private readonly IJsonService _jsonService;
        
        private const string ControllerApi =  "/deployment/v1";

        
        public DeploymentService(Configuration configuration, ILogger logger, RestApiClient restApiClient, IJsonService jsonService)
        {
            _configuration = configuration;
            _logger = logger;
            _restApi = restApiClient;
            _jsonService = jsonService;
        }

        public IBaseRestApiOperation ResolveBranchAsync(string environmentId, string version)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/resolve-branch";
            
            var response = _restApi.Post<ResolveBranchResponseDto>(route, new ResolveBranchRequestDto()
            {
                EnvironmentId = environmentId,
                ClientVersion = version,
            });

            response.UseCompletedCallback(result =>
            {
                _logger.Log(result.DownloadHandler.text);
            });
            
            return response;
        }
    }
}