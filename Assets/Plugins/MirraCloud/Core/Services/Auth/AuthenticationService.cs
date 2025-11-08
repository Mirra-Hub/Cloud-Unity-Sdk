using System;
using System.Collections.Generic;
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
        public event Action<PlayerAccountInfo> OnLogin;

        private string _authToken;

        private const string SERVICE_ROUTE = "/cloud/players/auth/v1";

        private string _guestId;
        private readonly Configuration _configuration;
        private const string GUEST_ID_KEY = "GuestId";

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
            string route = SERVICE_ROUTE + "web/login";
            var data = new LoginWithWebIDRequest()
            {
                Id = webId,
                CreateAccount = createAccount,
            };
                
            var request = _restApi.Post(route, data);

            request.UseCompletedCallback(LoginResponse);

            return request;
        }

        public IRestApiOperation LoginWithDeviceIDAsync(string deviceId, bool createAccount = true)
        {
            string route = $"{SERVICE_ROUTE}/{_configuration.ProjectId}/device/login";

            var data = new LoginWithDeviceIDRequest()
            {
                DeviceId = deviceId,
                CreateAccount = createAccount,
            };
                
            var request = _restApi.Post(route, data, new RequestOptions()
            {
                Headers = new Dictionary<string, string>() {
                    {"projectToken", _configuration.Token}
                },
            });

            request.UseCompletedCallback(LoginResponse);

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
            
            string route = SERVICE_ROUTE + "guest/login";
            var data = new LoginWithGuestRequest()
            {
                GuestId = _guestId,
                CreateAccount = createAccount,
            };
                
            var request = _restApi.Post(route, data);

            request.UseCompletedCallback(LoginResponse);

            return request;
        }

        private void LoginResponse(RestApiOperation response)
        {
            if (response.IsSuccess)
            {
                Debug.Log(response.DownloadHandler.text);
                AuthDto authDto = response.GetData<AuthDto>();
                
                _authToken = authDto.token;
                IsAuth = true;
                
                OnLogin?.Invoke(new PlayerAccountInfo(authDto.playerInfo));
            }
        }

        private void AuthTokenInterceptor(UnityWebRequest request)
        {
            request.SetRequestHeader("Authorization", "Bearer " + _authToken);
        }
    }
}