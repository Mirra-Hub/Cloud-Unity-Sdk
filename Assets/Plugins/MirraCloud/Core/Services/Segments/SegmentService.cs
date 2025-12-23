using System.Collections.Generic;
using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using Plugins.MirraCloud.Core.Services.Segments.Dto;
using UnityEngine;
using ILogger = MirraCloud.Core.Logger.ILogger;

namespace Plugins.MirraCloud.Core.Services.Segments
{
    public class SegmentService
    {
        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private readonly RestApiClient _restApi;
        private readonly IJsonService _jsonService;

        private const string ControllerApi = "/segments/v1";

        private readonly Dictionary<string, SegmentDto> _segments = new Dictionary<string, SegmentDto>();
        private readonly List<SegmentDto> _playerSegments = new List<SegmentDto>();

        public SegmentService(Configuration configuration, ILogger logger, RestApiClient restApiClient, IJsonService jsonService)
        {
            _configuration = configuration;
            _logger = logger;
            _restApi = restApiClient;
            _jsonService = jsonService;
        }

        public AsyncOperation<RestApiResult<SegmentDto[]>> LoadConfigAsync()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/segments";

            var op = _restApi.GetAsync<SegmentDto[]>(route);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess || completed.Result.Data == null)
                {
                    _logger.Error(completed.Result.Error?.Message ?? "Segments request failed.");
                    return;
                }

                _segments.Clear();
                _playerSegments.Clear();

                foreach (var segment in completed.Result.Data)
                {
                    if (segment == null || string.IsNullOrEmpty(segment.id))
                    {
                        continue;
                    }

                    _segments[segment.id] = segment;
                }

                _logger.Log(_jsonService.ToJson(completed.Result.Data));
                CalculateSegments();
            };

            return op;
        }

        private void CalculateSegments()
        {
            Debug.Log("calculate segments");

            foreach (var segment in _segments.Values)
            {
                if (segment.isEnable)
                {
                    Debug.Log($"calculate segment {segment.name}");

                    var ruleResult = MirraCloudSDK.RuleConstructor.ExecuteRule(segment.ruleTreeId);
                    if (ruleResult)
                    {
                        _playerSegments.Add(segment);
                    }
                }
            }
        }
    }
}
