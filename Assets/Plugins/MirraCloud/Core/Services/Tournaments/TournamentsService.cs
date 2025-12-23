using System;
using System.Collections.Generic;
using MirraCloud;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.Services.Tournaments.Dto;

namespace Plugins.MirraCloud.Core.Services.Tournaments
{
    public class TournamentsService 
    {
        private readonly Configuration _configuration;
        private readonly RestApiClient _restApi;
        private readonly List<TournamentConfig> _tournamentConfigs = new List<TournamentConfig>();

        private const string ControllerApi = "/tournaments/v1";

        public bool IsInitialized { get; private set; }
        public IReadOnlyList<TournamentConfig> TournamentConfigs => _tournamentConfigs;
        
        public TournamentsService(Configuration configuration, RestApiClient restApi)
        {
            _restApi = restApi;
            _configuration = configuration;
        }

        public IBaseRestApiOperation Initialize()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/tournaments";

            var operation = _restApi.Get<TournamentConfigDto[]>(route);

            operation.UseCompletedCallback(response =>
            {
                
                _tournamentConfigs.Clear();

                if (response.IsSuccess)
                {
                    foreach (var tournamentConfigDto in response.Value)
                    {
                        _tournamentConfigs.Add(new TournamentConfig(tournamentConfigDto));
                    }
                }

                IsInitialized = true;
            });
            
            return operation;
        }

        public IRestApiOperation<TournamentConfigDto> GetConfigAsync(string tournamentId)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/tournaments/{tournamentId}";
            return _restApi.Get<TournamentConfigDto>(route);
        }

        public IRestApiOperation<TournamentEntriesDto> GetTopAsync(string tournamentId, string tableId, int entriesCount = 100)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/tournaments/{tournamentId}/entries/top?tableId={tableId}&entriesCount={entriesCount}";
            return _restApi.Get<TournamentEntriesDto>(route);
        }

        public IRestApiOperation<TournamentEntriesDto> GetTopByCountryAsync(string tournamentId, string tableId, int entriesCount = 100)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/tournaments/{tournamentId}/entries/top-by-country?tableId={tableId}&entriesCount={entriesCount}";
            return _restApi.Get<TournamentEntriesDto>(route);
        }

        public IRestApiOperation<TournamentEntriesDto> GetTopByFriendsAsync(string tournamentId, string tableId, string[] friendIds)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/tournaments/{tournamentId}/entries/top-by-friends?tableId={tableId}";
            var dto = new FriendsTopRequestDto { friendIds = friendIds ?? Array.Empty<string>() };
            return _restApi.Post<TournamentEntriesDto>(route, dto);
        }

        public IRestApiOperation<TournamentPlayersAroundDto> GetAroundAsync(string tournamentId, string tableId, int entriesRange = 10)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/tournaments/{tournamentId}/entries/around?tableId={tableId}&entriesRange={entriesRange}";
            return _restApi.Get<TournamentPlayersAroundDto>(route);
        }

        public IRestApiOperation<TournamentTopAndPlayersAroundDto> GetTopAndAroundAsync(string tournamentId, string tableId, int topEntriesCount = 100, int aroundEntriesRange = 10)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/tournaments/{tournamentId}/entries/top-and-around?tableId={tableId}&topEntriesCount={topEntriesCount}&aroundEntriesRange={aroundEntriesRange}";
            return _restApi.Get<TournamentTopAndPlayersAroundDto>(route);
        }

        public IRestApiOperation<TournamentEntryDto> GetPlayerAsync(string tournamentId, string tableId)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/tournaments/{tournamentId}/entries?tableId={tableId}";
            return _restApi.Get<TournamentEntryDto>(route);
        }

        public IRestApiOperation SubmitScoreAsync(string tournamentId, double score, string playerName = null)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/tournaments/{tournamentId}/entries";

            var name = playerName ?? MirraCloudSDK.PlayerAccount?.PlayerAccountInfo?.Nickname ?? string.Empty;

            var dto = new SubmitScoreDto
            {
                playerName = name,
                value = score,
            };

            return _restApi.Post(route, dto);
        }

        public IRestApiOperation<PlayerLeagueMetaDto> GetPlayerLeagueMetaAsync(string tournamentId)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/tournaments/{tournamentId}/players-league";
            return _restApi.Get<PlayerLeagueMetaDto>(route);
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
