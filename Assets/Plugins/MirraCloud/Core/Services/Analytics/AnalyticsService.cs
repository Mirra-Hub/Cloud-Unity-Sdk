using System.Globalization;
using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Core.Logger;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.Services.Analytics.Dto;

namespace Plugins.MirraCloud.Core.Services.Analytics
{
    public class AnalyticsService
    {
        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private readonly RestApiClient _restApi;
        private readonly IJsonService _jsonService;
        
        private const string ControllerApi =  "/analytics/v1";

        
        public AnalyticsService(Configuration configuration, ILogger logger, RestApiClient restApiClient, IJsonService jsonService)
        {
            _configuration = configuration;
            _logger = logger;
            _restApi = restApiClient;
            _jsonService = jsonService;
        }

        public IBaseRestApiOperation SendEvent(string metricId)
        {
            return SubmitEvent(metricId);
        }
        
        public IBaseRestApiOperation SendEvent(string metricId, string value)
        {
            return SubmitEvent(metricId, value);
        }
        
        public IBaseRestApiOperation SendEvent(string metricId, int value)
        {
            return SubmitEvent(metricId, value.ToString());
        }
        
        public IBaseRestApiOperation SendEvent(string metricId, float value)
        {
            return SubmitEvent(metricId, value.ToString(CultureInfo.InvariantCulture));
        }
        
        public IBaseRestApiOperation SendEvent(string metricId, bool value)
        {
            return SubmitEvent(metricId, value.ToString());
        }
        
        private IBaseRestApiOperation SubmitEvent(string metricId, string value = null)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/platforms/{_configuration.AnalyticsPlatformId}/custom-metrics/{metricId}";
            
            var response = _restApi.Post(route, new SendEventDto()
            {
                Value = value,
            });

            return response;
        }
    }
}