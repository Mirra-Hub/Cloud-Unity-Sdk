using System.Collections.Generic;
using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.General.AsyncOperations;
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

        private const string ControllerApi = "/rules-constructor/v1";

        private readonly Dictionary<string, RulesTreeDto> _rules = new Dictionary<string, RulesTreeDto>();

        public RuleConstructorService(Configuration configuration, ILogger logger, RestApiClient restApiClient, IJsonService jsonService)
        {
            _configuration = configuration;
            _logger = logger;
            _restApi = restApiClient;
            _jsonService = jsonService;
        }

        public AsyncOperation<RestApiResult<RulesTreeDto[]>> LoadConfigAsync()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/rules";

            var op = _restApi.GetAsync<RulesTreeDto[]>(route);
            op.OnCompleted += completed =>
            {
                if (!completed.Result.IsSuccess || completed.Result.Data == null)
                {
                    _logger.Error(completed.Result.Error?.Message ?? "RulesConstructor request failed.");
                    return;
                }

                _rules.Clear();

                foreach (var rulesTree in completed.Result.Data)
                {
                    if (rulesTree == null || string.IsNullOrEmpty(rulesTree.id))
                    {
                        continue;
                    }

                    _rules[rulesTree.id] = rulesTree;
                }

                Debug.Log(_jsonService.ToJson(completed.Result.Data));
            };

            return op;
        }

        public bool ExecuteRule(string ruleId)
        {
            if (_rules.TryGetValue(ruleId, out var rule))
            {
                return rule.root.Execute();
            }

            Debug.LogError($"not found rule from id: {ruleId}");
            return false;
        }
    }
}
