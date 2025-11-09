using System;
using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Core.Auth;
using MirraCloud.Core.Logger;

namespace Plugins.MirraCloud.Core.Services.PlayerAccount
{
    public class PlayerAccountService : IDisposable
    {
        private readonly RestApiClient _restApi;
        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private AuthenticationService _authenticationService;

        public PlayerAccountInfo PlayerAccountInfo { get; private set; }

        private const string SERVICE_ROUTE = "/player-accounts/v1/projects";
        
        public PlayerAccountService(AuthenticationService authenticationService, RestApiClient restApi, Configuration configuration, ILogger logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
            _restApi = restApi;
            _configuration = configuration;

            _authenticationService.OnLogin += SetupInfo;
        }

        public void Dispose()
        {
            _authenticationService.OnLogin -= SetupInfo;
        }

        public RestApiOperation<PlayerAccountInfo> UpdatePlayerInfo(UpdatePlayerInfoOptions options)
        {
            string route = $"{SERVICE_ROUTE}/{_configuration.ProjectId}";
            var data = new UpdatePlayerInfoDto()
            {
                name = options.Name ?? PlayerAccountInfo.Name,
                age = options.Age ?? PlayerAccountInfo.Age,
            };
                
            var request = _restApi.Post<PlayerAccountInfo>(route, data);

            request.UseExtractData(response =>
            {
                var dto = response.GetData<PlayerAccountInfoDto>();

                PlayerAccountInfo = new PlayerAccountInfo(dto);
                
                return PlayerAccountInfo;
            });

            return request;
        }

        private void SetupInfo(PlayerAccountInfo info)
        {
            PlayerAccountInfo = info;
        }
    }
}