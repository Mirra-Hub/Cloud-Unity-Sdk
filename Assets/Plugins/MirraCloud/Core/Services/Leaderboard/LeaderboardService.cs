using System;
using System.Collections.Generic;
using MirraCloud.Core.Leaderboard.Dto;
using MirraCloud.Core.Leaderboard.Entities;
using MirraCloud.Core.Logger;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.Services.PlayerAccount;

namespace MirraCloud.Core.Leaderboard
{
    public class LeaderboardService 
    {        
        private const string ControllerApi = "/leaderboards/v1";
        
        private readonly IJsonService _jsonService;
        private readonly ILogger _logger;
        private readonly RestApiClient _restApi;
        private readonly Configuration _configuration;
        private readonly PlayerAccountService _playerAccountService;

        private readonly List<LeaderboardConfig> _leaderboardConfigs = new List<LeaderboardConfig>();
        public IReadOnlyList<LeaderboardConfig> LeaderboardConfigs => _leaderboardConfigs;

        public LeaderboardService(Configuration configuration, PlayerAccountService playerAccountService, ILogger logger, IJsonService jsonService, RestApiClient restApi) 
        {
            _configuration = configuration;
            _playerAccountService = playerAccountService;
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

        public IRestApiOperation<LeaderboardConfigDto> GetConfigAsync(string leaderboardId)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}";
            return _restApi.Get<LeaderboardConfigDto>(route);
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
                PlayerName = _playerAccountService.PlayerAccountInfo.Nickname,
                Value = score,
            };
            
            var operation = _restApi.Post(route, submitScoreDto);

            return operation; 
        }
        
        public IRestApiOperation<LeaderboardEntriesDto> GetLeaderboardTopEntries(string leaderboardId, int top = 100)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries/top?entriesCount={top}";

            var operation = _restApi.Get<LeaderboardEntriesDto>(route);

            return operation;
        }

        public IRestApiOperation<LeaderboardEntriesDto> GetLeaderboardTopEntriesByCountry(string leaderboardId, int entriesCount = 100)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries/top-by-country?entriesCount={entriesCount}";
            return _restApi.Get<LeaderboardEntriesDto>(route);
        }

        public IRestApiOperation<LeaderboardEntriesDto> GetLeaderboardTopEntriesByFriends(string leaderboardId, string[] friendIds)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries/top-by-friends";
            var dto = new FriendsTopRequestDto { FriendIds = friendIds ?? Array.Empty<string>() };
            return _restApi.Post<LeaderboardEntriesDto>(route, dto);
        }
        
        public IRestApiOperation<LeaderboardAroundEntriesDto> GetLeaderboardPlayerAroundEntries(string leaderboardId, int around = 10)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries/around?entriesRange={around}";

            var operation = _restApi.Get<LeaderboardAroundEntriesDto>(route);

            return operation;
        }
        
        public IRestApiOperation<LeaderboardTopAndPlayersAroundDto> GetLeaderboardEntries(string leaderboardId, int top = 100, int around = 10)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries/top-and-around?topEntriesCount={top}&aroundEntriesRange={around}";

            var operation = _restApi.Get<LeaderboardTopAndPlayersAroundDto>(route);

            return operation;
        }
        
        public IRestApiOperation<LeaderboardEntryDto> GetLeaderboardPlayer(string leaderboardId)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/leaderboards/{leaderboardId}/entries";

            var operation = _restApi.Get<LeaderboardEntryDto>(route);

            return operation;
        }

        public IRestApiOperation<PlayerRewardsDto> GetRewardsAsync(bool reset = true)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/rewards?reset={reset.ToString().ToLowerInvariant()}";
            return _restApi.Get<PlayerRewardsDto>(route);
        }

        public IRestApiOperation SubmitRewardsAsync()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/rewards";
            return _restApi.Post(route, new { });
        }
    }
}
