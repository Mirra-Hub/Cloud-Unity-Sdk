using System;
using System.Collections.Generic;
using MirraCloud.Core;
using MirraCloud.Core.Storage;
using MirraCloud.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace MirraCloud.Core.Auth
{
    public class AuthenticationService
    {
        private readonly Logger.ILogger _logger;
        private readonly IStorage _storage;
        private readonly RestApiClient _restApi;
        private readonly Configuration _configuration;

        private const string AUTH_ROUTE = "/players/auth/v1/projects";
        private const string AUTH_LINK_ROUTE = "/players/link/v1/projects";
        private const string ACCOUNTS_ROUTE = "/players/accounts/v1/projects";

        private const string GUESTID_KEY = "GuestId";
        private const string REFRESH_TOKEN_KEY = "RefreshToken";
        private const string SESSIONID_KEY = "SessionId";
        private const string SESSION_EXPIRESAT_KEY = "SessionExpiresAt";

        private string _authToken;
        private string _sessionId;
        private string _refreshToken;
        private DateTime _sessionExpiresAt;

        public bool IsAuth { get; private set; }

        public event Action<GetAuthDataDto> OnLogin;
        public event Action<GetAuthDataDto> OnAuthConflict;
        public event Action OnSessionRefreshed;
        public event Action OnSessionExpired;

        public AuthenticationService(Configuration configuration, Logger.ILogger logger, IStorage storage, RestApiClient restApi)
        {
            _configuration = configuration;
            _restApi = restApi;
            _logger = logger;
            _storage = storage;

            _restApi.UseRequestInterceptor(AuthTokenInterceptor);
            _restApi.UseResponseInterceptor(SessionRefreshInterceptor);
        }
        
        public IRestApiOperation InitializeAsync()
        {
            if (_storage.HasKey(REFRESH_TOKEN_KEY) == false)
            {
                var emptyOperation = new RestApiOperation(_restApi.JsonService);
                var dummyRequest = new UnityWebRequest(_restApi.GetUrl(string.Empty), UnityWebRequest.kHttpVerbGET)
                {
                    downloadHandler = new DownloadHandlerBuffer()
                };
                emptyOperation.Initialize(dummyRequest);
                emptyOperation.Complete();
                return emptyOperation;
            }

            var savedRefresh = _storage.GetString(REFRESH_TOKEN_KEY);
            if (string.IsNullOrWhiteSpace(savedRefresh))
            {
                ClearSessionStorage();

                var emptyOperation = new RestApiOperation(_restApi.JsonService);
                var dummyRequest = new UnityWebRequest(_restApi.GetUrl(string.Empty), UnityWebRequest.kHttpVerbGET)
                {
                    downloadHandler = new DownloadHandlerBuffer()
                };
                emptyOperation.Initialize(dummyRequest);
                emptyOperation.Complete();
                return emptyOperation;
            }

            _refreshToken = savedRefresh;

            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/session";
            var dto = new RefreshSessionDto
            {
                RefreshToken = _refreshToken
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }
        
        #region Login

        public IRestApiOperation LoginGuestAsync(bool createAccount = true)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/guest";
            var dto = new LoginAsGuestDto
            {
                CreateAccount = createAccount
            };

            if (_storage.HasKey(GUESTID_KEY))
            {
                dto.GuestId = _storage.GetString(GUESTID_KEY);
            }
            
            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        public IRestApiOperation LoginDeviceAsync(string deviceId, bool createAccount = true)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/device";
            var dto = new LoginByDeviceIdDto
            {
                DeviceId = deviceId,
                CreateAccount = createAccount
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        public IRestApiOperation LoginEmailAsync(string email, string password, bool createAccount = true)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/email";
            var dto = new LoginByEmailDto
            {
                Email = email,
                Password = password,
                CreateAccount = createAccount
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        public IRestApiOperation LoginUsernameAsync(string username, string password, bool createAccount = true)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/username";
            var dto = new LoginByUsernameDto
            {
                Login = username,
                Password = password,
                CreateAccount = createAccount
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        public IRestApiOperation LoginVkGamesAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/vk-games", userId, createAccount);
        }

        public IRestApiOperation LoginYandexGamesAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/yandex-games", userId, createAccount);
        }

        public IRestApiOperation LoginGooglePlayAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/google-play", userId, createAccount);
        }

        public IRestApiOperation LoginAppleGameCenterAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/apple-game-center", userId, createAccount);
        }

        public IRestApiOperation LoginGoogleAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/google", userId, createAccount);
        }

        public IRestApiOperation LoginAppleAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/apple", userId, createAccount);
        }

        public IRestApiOperation LoginYandexAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/yandex", userId, createAccount);
        }

     
        public IRestApiOperation StartOpenIdLoginAsync(int providerId, string successUrl)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/openid/{providerId}";
            var dto = new RegisterOpenIdProviderDto
            {
                SuccessUrl = successUrl
            };

            return _restApi.Post(route, dto);
        }

        private IRestApiOperation LoginByUserIdAsync(string route, string userId, bool createAccount)
        {
            var dto = new LoginByUserIdDto
            {
                UserId = userId,
                CreateAccount = createAccount
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        #endregion

        #region Link

        public IRestApiOperation LinkGuestAsync(bool createAccount = false)
        {
            var route = $"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/guest";
            var dto = new LoginAsGuestDto
            {
                CreateAccount = createAccount
            };
            
            if (_storage.HasKey(GUESTID_KEY))
            {
                dto.GuestId = _storage.GetString(GUESTID_KEY);
            }

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        public IRestApiOperation LinkDeviceAsync(string deviceId, bool createAccount = false)
        {
            var route = $"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/device";
            var dto = new LoginByDeviceIdDto
            {
                DeviceId = deviceId,
                CreateAccount = createAccount
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        public IRestApiOperation LinkEmailAsync(string email, string password, bool createAccount = false)
        {
            var route = $"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/email";
            var dto = new LoginByEmailDto
            {
                Email = email,
                Password = password,
                CreateAccount = createAccount
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        public IRestApiOperation LinkUsernameAsync(string username, string password, bool createAccount = false)
        {
            var route = $"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/username";
            var dto = new LoginByUsernameDto
            {
                Login = username,
                Password = password,
                CreateAccount = createAccount
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        public IRestApiOperation LinkVkGamesAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/vk-games", userId, createAccount);
        }

        public IRestApiOperation LinkYandexGamesAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/yandex-games", userId, createAccount);
        }

        public IRestApiOperation LinkGooglePlayAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/google-play", userId, createAccount);
        }

        public IRestApiOperation LinkAppleGameCenterAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/apple-game-center", userId, createAccount);
        }

        public IRestApiOperation LinkGoogleAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/google", userId, createAccount);
        }

        public IRestApiOperation LinkAppleAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/apple", userId, createAccount);
        }

        public IRestApiOperation LinkYandexAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/yandex", userId, createAccount);
        }

        public IRestApiOperation LinkOpenIdAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/openid", userId, createAccount);
        }

        private IRestApiOperation LinkByUserIdAsync(string route, string userId, bool createAccount)
        {
            var dto = new LoginByUserIdDto
            {
                UserId = userId,
                CreateAccount = createAccount
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        public IRestApiOperation ResolveLinkConflictAsync(int providerType, string targetAccountId)
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/link-conflict/resolve";
            var dto = new LinkAuthProviderDto
            {
                ProviderType = providerType,
                TargetAccountId = targetAccountId
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(HandleAuthCompleted);
            return op;
        }

        #endregion

        #region Session

        public IRestApiOperation RefreshSessionAsync()
        {
            if (string.IsNullOrEmpty(_refreshToken))
            {
                _logger.Log("RefreshSessionAsync called without refresh token.");
            }

            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/refresh";
            var dto = new RefreshSessionDto
            {
                RefreshToken = _refreshToken
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(operation =>
            {
                if (!operation.IsSuccess)
                {
                    HandleSessionExpired();
                    return;
                }

                var result = operation.GetData<SessionRefreshResultDto>();
                if (result == null || result.Session == null)
                {
                    HandleSessionExpired();
                    return;
                }

                ApplySession(result.Session);
                OnSessionRefreshed?.Invoke();
            });

            return op;
        }

        public IRestApiOperation LogoutAsync()
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/logout";
            var dto = new RefreshSessionDto
            {
                RefreshToken = _refreshToken
            };

            var op = _restApi.Post(route, dto);
            op.UseCompletedCallback(_ =>
            {
                ClearSession();
                OnSessionExpired?.Invoke();
            });
            return op;
        }

        public IRestApiOperation LogoutAllAsync()
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/logout/all";
            var op = _restApi.Post(route, new { });
            op.UseCompletedCallback(_ =>
            {
                ClearSession();
                OnSessionExpired?.Invoke();
            });
            return op;
        }

        #endregion

        #region Internal handlers

        private void HandleAuthCompleted(RestApiOperation operation)
        {
            if (operation.IsSuccess == false)
            {
                _logger.Error(operation.ErrorMessage);
                return;
            }

            var data = operation.GetData<GetAuthDataDto>();
            if (data == null)
            {
                _logger.Error("Empty auth response");
                return;
            }

            if (data.Status == AuthResultStatus.Conflict)
            {
                OnAuthConflict?.Invoke(data);
                return;
            }

            if (string.IsNullOrEmpty(data.Token) || data.Session == null)
            {
                _logger.Error("Auth response without token or session");
                return;
            }

            _authToken = data.Token;
            ApplySession(data.Session);
            IsAuth = true;
            if (string.IsNullOrEmpty(data.GuestId) == false)
            {
                _storage.SaveString(GUESTID_KEY, data.GuestId);
            }
            SaveSessionToStorage();

            OnLogin?.Invoke(data);
        }

        private void ApplySession(SessionInfoDto session)
        {
            _sessionId = session.SessionId;
            _refreshToken = session.RefreshToken;
            _sessionExpiresAt = session.ExpiresAt;
        }

        private void ClearSession()
        {
            _authToken = null;
            _sessionId = null;
            _refreshToken = null;
            _sessionExpiresAt = default;
            IsAuth = false;
        }

        private void SaveSessionToStorage()
        {
            if (!string.IsNullOrEmpty(_refreshToken))
            {
                _storage.SaveString(REFRESH_TOKEN_KEY, _refreshToken);
            }

            if (!string.IsNullOrEmpty(_sessionId))
            {
                _storage.SaveString(SESSIONID_KEY, _sessionId);
            }

            _storage.SaveString(SESSION_EXPIRESAT_KEY, _sessionExpiresAt.ToString("o"));
        }

        private void ClearSessionStorage()
        {
            _storage.DeleteKeys(REFRESH_TOKEN_KEY, SESSIONID_KEY, SESSION_EXPIRESAT_KEY);
        }

        private RestRequestConfig AuthTokenInterceptor(RestRequestConfig config)
        {
            //  new Claim(nameof(profile.Username), profile?.Username ?? string.Empty),
            //       new Claim(nameof(profile.IconKey), JsonConvert.SerializeObject(profile?.IconKey ?? new IconKey()))
            //  new Claim(nameof(account.Age), account.Age.ToString()),
            //   new Claim(nameof(account.Country), account.Country.ToString()),
            //  new Claim(nameof(account.LanguageCode), account.LanguageCode.ToString()),
            //     new Claim("Account" + nameof(account.SegmentIds), string.Join(',', account.SegmentIds)),
            //    new Claim("Profile" + nameof(profile.SegmentIds), string.Join(',', profile?.SegmentIds ?? [])),
            
            if (!string.IsNullOrEmpty(_authToken))
            {
                config.Headers ??= new Dictionary<string, string>();
                config.Headers["Authorization"] = "Bearer " + _authToken;
            }

            return config;
        }

        private System.Collections.IEnumerator SessionRefreshInterceptor(RestResponseContext context)
        {
            var code = context.Request.responseCode;
            if ((code == 401 || code == 403) && !string.IsNullOrEmpty(_refreshToken))
            {
                var refreshOperation = RefreshSessionAsync();
                yield return refreshOperation.Task;

                if (refreshOperation.IsSuccess)
                {
                    context.RetryRequested = true;
                    SaveSessionToStorage();
                }
                else
                {
                    HandleSessionExpired();
                }
            }
        }

        private void HandleSessionExpired()
        {
            ClearSession();
            ClearSessionStorage();
            OnSessionExpired?.Invoke();
        }

        #endregion
    }
}
