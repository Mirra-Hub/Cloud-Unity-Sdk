using System.Globalization;
using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Core.Logger;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using Plugins.MirraCloud.Core.Services.Analytics.Dto;

namespace Plugins.MirraCloud.Core.Services.Analytics
{
    public class AnalyticsService
    {
        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private readonly RestApiClient _restApi;

        private const string ControllerApi = "/analytics/v1";

        public AnalyticsService(Configuration configuration, ILogger logger, RestApiClient restApiClient)
        {
            _configuration = configuration;
            _logger = logger;
            _restApi = restApiClient;
        }

        public AsyncOperation<RestApiResult> SendEventAsync(string metricId)
        {
            return SubmitEventAsync(metricId);
        }

        public AsyncOperation<RestApiResult> SendEventAsync(string metricId, string value)
        {
            return SubmitEventAsync(metricId, value);
        }

        public AsyncOperation<RestApiResult> SendEventAsync(string metricId, int value)
        {
            return SubmitEventAsync(metricId, value.ToString());
        }

        public AsyncOperation<RestApiResult> SendEventAsync(string metricId, float value)
        {
            return SubmitEventAsync(metricId, value.ToString(CultureInfo.InvariantCulture));
        }

        public AsyncOperation<RestApiResult> SendEventAsync(string metricId, bool value)
        {
            return SubmitEventAsync(metricId, value.ToString());
        }

        private AsyncOperation<RestApiResult> SubmitEventAsync(string metricId, string value = null)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/platforms/{_configuration.AnalyticsPlatformId}/custom-metrics/{metricId}";

            var response = _restApi.PostAsync(route, new SendEventDto { Value = value });
            response.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess)
                {
                    _logger.Error(completed.Result.Error?.Message ?? "Analytics request failed.");
                }
            };

            return response;
        }
    }
}
