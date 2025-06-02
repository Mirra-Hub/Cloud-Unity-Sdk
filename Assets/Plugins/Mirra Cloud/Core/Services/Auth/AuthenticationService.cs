using System;
using System.IO;
using MirraCloud.Core.Storage;
using UnityEngine;
using UnityEngine.Networking;

namespace MirraCloud.Core.Auth
{
    public class AuthenticationService
    {
        private readonly Logger.ILogger _logger;
        private readonly IStorage _storage;
        private readonly RestApiClient _restApi;

        public bool IsAuth { get; private set; }
        private string _authToken;

        private const string SERVICE_ROUTE = "/auth/v1";

        private string _guestId;
        private Configuration _configuration;
        private const string GUEST_ID_KEY = "GuestId";

        public PlayerAccountInfo PlayerAccountInfo { get; private set; }
        
        public AuthenticationService(Configuration configuration, Logger.ILogger logger, IStorage storage, RestApiClient restApi)
        {
            _configuration = configuration;
            _restApi = restApi;
            _logger = logger;
            _storage = storage;
            
            _restApi.UseRequestInterceptor(AuthTokenInterceptor);
        }

        public IRestApiOperation LoginWithWebIDAsync(string webId, bool createAccount = true)
        {
            string route = SERVICE_ROUTE + "login-webId";
            var data = new LoginWithWebIDRequest()
            {
                Id = webId,
                CreateAccount = createAccount,
            };
                
            var request = _restApi.Post(route, data);

            request.UseCompletedCallback(SetupToken);

            return request;
        }

        public IRestApiOperation LoginWithDeviceIDAsync(string deviceId, bool createAccount = true)
        {
            string route = $"{SERVICE_ROUTE}/{_configuration.ProjectId}/login-device";

            var data = new LoginWithDeviceIDRequest()
            {
                DeviceId = deviceId,
                CreateAccount = createAccount,
            };
                
            var request = _restApi.Post(route, data);

            request.UseCompletedCallback(SetupToken);

            return request;
        }

        public IRestApiOperation LoginWithGuestAsync(bool createAccount = true)
        {
            if (_storage.HasKey(GUEST_ID_KEY))
            {
                _guestId = _storage.GetString(GUEST_ID_KEY);
            }
            else
            {
                _guestId = Guid.NewGuid().ToString();
                _storage.SaveString(GUEST_ID_KEY, _guestId);
            }
            
            string route = SERVICE_ROUTE + "login-guest";
            var data = new LoginWithGuestRequest()
            {
                GuestId = _guestId,
                CreateAccount = createAccount,
            };
                
            var request = _restApi.Post(route, data);

            request.UseCompletedCallback(SetupToken);

            return request;
        }

        private void SetupToken(RestApiOperation response)
        {
            if (response.IsSuccess)
            {
                AuthResponse authResponse = response.GetData<AuthResponse>();
                
                _authToken = authResponse.token;
                IsAuth = true;
                PlayerAccountInfo = new PlayerAccountInfo(authResponse.playerInfo);
            }
        }

        private void AuthTokenInterceptor(UnityWebRequest request)
        {
            request.SetRequestHeader("Authorization", "Bearer " + _authToken);
        }
    }
}