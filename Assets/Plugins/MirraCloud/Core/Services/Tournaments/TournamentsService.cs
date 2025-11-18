using System.Collections.Generic;
using MirraCloud;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.Services.Tournaments.Dto;
using UnityEngine;

namespace Plugins.MirraCloud.Core.Services.Tournaments
{
    public class TournamentsService 
    {
        private readonly Configuration _configuration;
        private readonly RestApiClient _restApi;
        private readonly List<TournamentConfig> _tournamentConfigs = new List<TournamentConfig>();

        private const string ControllerApi = "/leaderboards/v1";

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
    }
}
