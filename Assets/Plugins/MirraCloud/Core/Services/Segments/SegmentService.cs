using System.Collections.Generic;
using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Json;
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

        private const string ControllerApi =  "/segments/v1";

        private readonly Dictionary<string, SegmentDto> _segments = new Dictionary<string, SegmentDto>();

        private readonly List<SegmentDto> _playerSegments = new List<SegmentDto>();
        
        public SegmentService(Configuration configuration, ILogger logger, RestApiClient restApiClient, IJsonService jsonService)
        {
            _configuration = configuration;
            _logger = logger;
            _restApi = restApiClient;
            _jsonService = jsonService;
        }
        
        public IRestApiOperation LoadConfigAsync()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/environments/{_configuration.BranchId}/segments";
            
            var response = _restApi.Get(route);

            response.UseCompletedCallback(result =>
            {
                _logger.Log(result.DownloadHandler.text);

                var segments = _jsonService.FromJson<SegmentDto[]>(result.DownloadHandler.text);

                foreach (var segment in segments)
                {
                    _segments.Add(segment.id, segment);
                }
                
                CalculateSegments();
            });
            
            return response;
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