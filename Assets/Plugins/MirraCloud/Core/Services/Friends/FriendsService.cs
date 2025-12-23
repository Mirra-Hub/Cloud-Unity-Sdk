using MirraCloud;
using MirraCloud.Core.Friends.Dto;
using MirraCloud.Core.Logger;

namespace MirraCloud.Core.Friends
{
    public class FriendsService
    {
        private readonly ILogger _logger;
        private readonly Configuration _configuration;
        private readonly RestApiClient _restApi;

        private const string ControllerApi = "/friends/v1/projects";

        public FriendsService(Configuration configuration, ILogger logger, RestApiClient restApi)
        {
            _configuration = configuration;
            _restApi = restApi;
            _logger = logger;
        }

        #region Friends

        public IRestApiOperation<GetPlayerDto[]> GetFriendsAsync()
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/friends";
            return _restApi.Get<GetPlayerDto[]>(route);
        }

        public IRestApiOperation RemoveFriendAsync(string targetPlayerId)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/friends/{targetPlayerId}";
            return _restApi.Delete(route);
        }

        public IRestApiOperation BanAsync(string targetPlayerId)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/ban/{targetPlayerId}";
            return _restApi.Post(route);
        }

        public IRestApiOperation BanManyAsync(string[] targetPlayerIds)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/ban";
            return _restApi.Post(route, targetPlayerIds);
        }

        #endregion

        #region Requests

        public IRestApiOperation SendAsync(string targetPlayerId)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/{targetPlayerId}/send";
            return _restApi.Post(route);
        }

        public IRestApiOperation SendManyAsync(string[] targetPlayerIds)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/send";
            return _restApi.Post(route, targetPlayerIds);
        }

        public IRestApiOperation RevokeAsync(string targetPlayerId)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/{targetPlayerId}/revoke";
            return _restApi.Post(route);
        }

        public IRestApiOperation RevokeManyAsync(string[] targetPlayerIds)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/revoke";
            return _restApi.Post(route, targetPlayerIds);
        }

        public IRestApiOperation AcceptAsync(string sourcePlayerId)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/{sourcePlayerId}/accept";
            return _restApi.Post(route);
        }

        public IRestApiOperation AcceptManyAsync(string[] sourcePlayerIds)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/accept";
            return _restApi.Post(route, sourcePlayerIds);
        }

        public IRestApiOperation RejectAsync(string sourcePlayerId)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/{sourcePlayerId}/reject";
            return _restApi.Post(route);
        }

        public IRestApiOperation RejectManyAsync(string[] sourcePlayerIds)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/reject";
            return _restApi.Post(route, sourcePlayerIds);
        }

        public IRestApiOperation<GetFriendRequestDto[]> GetRequestsAsync()
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests";
            return _restApi.Get<GetFriendRequestDto[]>(route);
        }

        public IRestApiOperation<GetFriendRequestDto[]> GetOutgoingAsync()
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/outgoing";
            return _restApi.Get<GetFriendRequestDto[]>(route);
        }

        public IRestApiOperation<GetFriendRequestDto[]> GetIncomingAsync()
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/incoming";
            return _restApi.Get<GetFriendRequestDto[]>(route);
        }

        public IRestApiOperation DeleteAsync(string targetPlayerId)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests/{targetPlayerId}";
            return _restApi.Delete(route);
        }

        public IRestApiOperation DeleteManyAsync(string[] targetPlayerIds)
        {
            var route = $"{ControllerApi}/{_configuration.ProjectId}/players/requests";
            var config = new RestRequestConfig
            {
                Body = targetPlayerIds
            };
            return _restApi.Delete(route, config);
        }

        #endregion
    }
}

