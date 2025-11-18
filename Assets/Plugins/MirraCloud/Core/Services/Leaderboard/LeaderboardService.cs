using System;
using System.Collections.Generic;
using MirraCloud.Core.Leaderboard.Dto;
using MirraCloud.Core.Leaderboard.Entities;
using MirraCloud.Core.Logger;
using MirraCloud.Json;

namespace MirraCloud.Core.Leaderboard
{
    public class LeaderboardService 
    {        
        private const string ControllerApi = "/leaderboards/v1";
        
        private readonly IJsonService _jsonService;
        private readonly ILogger _logger;
        private readonly RestApiClient _restApi;
        private readonly Configuration _configuration;

        private readonly List<LeaderboardConfig> _leaderboardConfigs = new List<LeaderboardConfig>();
        public IReadOnlyList<LeaderboardConfig> LeaderboardConfigs => _leaderboardConfigs;

        public LeaderboardService(Configuration configuration, ILogger logger, IJsonService jsonService, RestApiClient restApi) 
        {
            _configuration = configuration;
            _restApi = restApi;
            _logger = logger;
            _jsonService = jsonService;
        }

        public IBaseRestApiOperation Initialize()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards";

            var operation = _restApi.Get<LeaderboardConfigDto[]>(route);

            operation.UseCompletedCallback(response =>
            {
                _leaderboardConfigs.Clear();

                if (response.IsSuccess)
                {
                    foreach (var leaderboardConfigDto in response.Value)
                    {
                        _leaderboardConfigs.Add(new LeaderboardConfig(leaderboardConfigDto));
                    }
                }
            });
            
            return operation;
        }

        public IBaseRestApiOperation SubmitScore(DateTime score, string leaderboardId)
        {
            return SubmitScore(score.ToOADate(), leaderboardId);
        }
        
        public IBaseRestApiOperation SubmitScore(double score, string leaderboardId)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries";

            SubmitScoreDto submitScoreDto = new SubmitScoreDto()
            {
                PlayerName = "Petr",
                Value = score,
            };
            
            var operation = _restApi.Post(route, submitScoreDto);

            return operation; 
        }
        
        public IRestApiOperation<LeaderboardEntriesDto> GetLeaderboardTopEntries(string leaderboardId, int top = 100)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries/top";

            var operation = _restApi.Get<LeaderboardEntriesDto>(route);

            return operation;
        }
        
        public IRestApiOperation<LeaderboardAroundEntriesDto> GetLeaderboardPlayerAroundEntries(string leaderboardId, int around = 10)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries/around";

            var operation = _restApi.Get<LeaderboardAroundEntriesDto>(route);

            return operation;
        }
        
        public IRestApiOperation<LeaderboardTopAndPlayersAroundDto> GetLeaderboardEntries(string leaderboardId, int top = 100, int around = 10)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries/top-and-around";

            var operation = _restApi.Get<LeaderboardTopAndPlayersAroundDto>(route);

            return operation;
        }
        
        public IRestApiOperation<LeaderboardEntryDto> GetLeaderboardPlayer(string leaderboardId)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries";

            var operation = _restApi.Get<LeaderboardEntryDto>(route);

            return operation;
        }
    }
}
