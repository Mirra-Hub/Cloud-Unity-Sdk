using System;
using System.Collections.Generic;
using MirraCloud.Core.Storage;
using MirraCloud.Core.Auth.OpenId;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using UnityEngine;
using UnityEngine.Networking;

namespace MirraCloud.Core.Auth
{
    public class AuthenticationService : ISessionRefresher, ICloudSdkService
    {
        private readonly Logger.ILogger _logger;
        private readonly IStorage _storage;
        private readonly RestApiClient _restApi;
        private readonly Configuration _configuration;

        private const string AUTH_ROUTE = "/players/auth/v1/projects";
        private const string AUTH_LINK_ROUTE = "/players/link/v1/projects";
        private const string ACCOUNTS_ROUTE = "/players/accounts/v1/projects";
        private const string OPENID_RESULT_ROUTE = "/players/auth/v1/openid/result";

        private const string GUESTID_KEY = "GuestId";
        private const string REFRESH_TOKEN_KEY = "RefreshToken";
        private const string SESSIONID_KEY = "SessionId";
        private const string SESSION_EXPIRESAT_KEY = "SessionExpiresAt";
        
        private string _authToken;
        public string AuthToken => _authToken;
        
        private string _sessionId;
        private string _refreshToken;
        private DateTime _sessionExpiresAt;

        public bool IsAuth { get; private set; }
        public string SessionId => _sessionId;

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
            _restApi.SetSessionRefresher(this);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> InitializeAsync()
        {
            if (_storage.HasKey(REFRESH_TOKEN_KEY) == false)
            {
                return AsyncOperation<RestApiResult<GetAuthDataDto>>.CreateCompleted(RestApiResult<GetAuthDataDto>.Success(null));
            }

            var savedRefresh = _storage.GetString(REFRESH_TOKEN_KEY);
            if (string.IsNullOrWhiteSpace(savedRefresh))
            {
                ClearSessionAndStorage();
                return AsyncOperation<RestApiResult<GetAuthDataDto>>.CreateCompleted(RestApiResult<GetAuthDataDto>.Success(null));
            }

            _refreshToken = savedRefresh;

            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/session";
            var dto = new RefreshSessionDto { RefreshToken = _refreshToken };
            return PostAuthAsync(route, dto, noAuth: true);
        }

        #region Login

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginGuestAsync(bool createAccount = true)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/guest";
            var dto = new LoginAsGuestDto { CreateAccount = createAccount };

            if (_storage.HasKey(GUESTID_KEY))
            {
                dto.GuestId = _storage.GetString(GUESTID_KEY);
            }

            return PostAuthAsync(route, dto, noAuth: true);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginDeviceAsync(string deviceId, bool createAccount = true)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/device";
            var dto = new LoginByDeviceIdDto { DeviceId = deviceId, CreateAccount = createAccount };
            return PostAuthAsync(route, dto, noAuth: true);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginEmailAsync(string email, string password, bool createAccount = true)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/email";
            var dto = new LoginByEmailDto { Email = email, Password = password, CreateAccount = createAccount };
            return PostAuthAsync(route, dto, noAuth: true);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginUsernameAsync(string username, string password, bool createAccount = true)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/username";
            var dto = new LoginByUsernameDto { Login = username, Password = password, CreateAccount = createAccount };
            return PostAuthAsync(route, dto, noAuth: true);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginVkGamesAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/vk-games", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginYandexGamesAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/yandex-games", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginGooglePlayAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/google-play", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginAppleGameCenterAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/apple-game-center", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginGoogleAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/google", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginAppleAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/apple", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginYandexAsync(string userId, bool createAccount = true)
        {
            return LoginByUserIdAsync($"{AUTH_ROUTE}/{_configuration.ProjectId}/login/yandex", userId, createAccount);
        }

        public AsyncOperation<RestApiResult> StartOpenIdLoginAsync(int providerId, string successUrl)
        {
            var op = new AsyncOperation<RestApiResult>();
            var urlOp = BeginOpenIdLoginUrlAsync(providerId, successUrl);
            urlOp.UseCompleted(_ =>
            {
                if (!urlOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult.Fail(urlOp.Result.Error).WithMetaFrom(urlOp.Result));
                    return;
                }

                if (string.IsNullOrWhiteSpace(urlOp.Result.Data))
                {
                    op.Complete(RestApiResult.ValidationFail("OpenId auth url is empty.").WithMetaFrom(urlOp.Result));
                    return;
                }

                Application.OpenURL(urlOp.Result.Data);
                op.Complete(RestApiResult.Success().WithMetaFrom(urlOp.Result));
            });

            return op;
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> CompleteOpenIdLoginAsync(string openIdKey)
        {
            if (string.IsNullOrWhiteSpace(openIdKey))
            {
                return AsyncOperation<RestApiResult<GetAuthDataDto>>.CreateCompleted(RestApiResult<GetAuthDataDto>.ValidationFail("OpenId key is empty."));
            }

            return GetAuthAsync($"{OPENID_RESULT_ROUTE}/{openIdKey}", noAuth: true);
        }

        public AsyncOperation<RestApiResult<string>> BeginOpenIdLoginUrlAsync(int providerId, string successUrl)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/openid/{providerId}";
            return BeginOpenIdLoginUrlAsync(route, successUrl);
        }

        public AsyncOperation<RestApiResult<string>> BeginGoogleOpenIdLoginUrlAsync(string successUrl)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/google";
            return BeginOpenIdLoginUrlAsync(route, successUrl);
        }

        public AsyncOperation<RestApiResult<string>> BeginYandexOpenIdLoginUrlAsync(string successUrl)
        {
            var route = $"{AUTH_ROUTE}/{_configuration.ProjectId}/login/yandex";
            return BeginOpenIdLoginUrlAsync(route, successUrl);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginGoogleOpenIdAsync(OpenIdLoginOptions options = null)
        {
            return LoginOpenIdAsync(BeginGoogleOpenIdLoginUrlAsync, options);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginYandexOpenIdAsync(OpenIdLoginOptions options = null)
        {
            return LoginOpenIdAsync(BeginYandexOpenIdLoginUrlAsync, options);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LoginOpenIdAsync(int providerId, OpenIdLoginOptions options = null)
        {
            return LoginOpenIdAsync(successUrl => BeginOpenIdLoginUrlAsync(providerId, successUrl), options);
        }

        private AsyncOperation<RestApiResult<string>> BeginOpenIdLoginUrlAsync(string route, string successUrl)
        {
            if (string.IsNullOrWhiteSpace(successUrl))
            {
                return AsyncOperation<RestApiResult<string>>.CreateCompleted(RestApiResult<string>.ValidationFail("SuccessUrl is empty."));
            }

            var dto = new RegisterOpenIdProviderDto { SuccessUrl = successUrl };
            var config = new RestRequestConfig
            {
                NoAuth = true,
                DisableRetry = true,
                RedirectLimit = 0,
                AllowedHttpStatusCodes = new long[] { 301, 302, 303, 307, 308 }
            };

            return _restApi.PostAsync<string>(route, dto, config, ExtractRedirectLocation);
        }

        private AsyncOperation<RestApiResult<GetAuthDataDto>> LoginOpenIdAsync(Func<string, AsyncOperation<RestApiResult<string>>> beginLoginUrlAsync, OpenIdLoginOptions options)
        {
            var op = new AsyncOperation<RestApiResult<GetAuthDataDto>>();

            if (!OpenIdCallbackReceiverFactory.TryCreate(options, out var receiver, out var receiverError))
            {
                op.Complete(RestApiResult<GetAuthDataDto>.ValidationFail(receiverError));
                return op;
            }

            var beginOp = beginLoginUrlAsync(receiver.SuccessUrl);
            beginOp.UseCompleted(_ =>
            {
                if (!beginOp.Result.IsSuccess)
                {
                    receiver.Dispose();
                    op.Complete(RestApiResult<GetAuthDataDto>.Fail(beginOp.Result.Error).WithMetaFrom(beginOp.Result));
                    return;
                }

                if (string.IsNullOrWhiteSpace(beginOp.Result.Data))
                {
                    receiver.Dispose();
                    op.Complete(RestApiResult<GetAuthDataDto>.ValidationFail("OpenId auth url is empty.").WithMetaFrom(beginOp.Result));
                    return;
                }

                var waitOp = receiver.WaitForKeyAsync();
                waitOp.UseCompleted(_ =>
                {
                    receiver.Dispose();

                    if (string.IsNullOrWhiteSpace(waitOp.Result))
                    {
                        op.Complete(RestApiResult<GetAuthDataDto>.ValidationFail("OpenId callback key was not received."));
                        return;
                    }

                    var completeOp = CompleteOpenIdLoginAsync(waitOp.Result);
                    completeOp.UseCompleted(completed => op.Complete(completed.Result));
                });

                Application.OpenURL(beginOp.Result.Data);
            });

            return op;
        }

        private static string ExtractRedirectLocation(UnityWebRequest request)
        {
            return request.GetResponseHeader("Location") ?? request.GetResponseHeader("location");
        }

        private AsyncOperation<RestApiResult<GetAuthDataDto>> LoginByUserIdAsync(string route, string userId, bool createAccount)
        {
            var dto = new LoginByUserIdDto { UserId = userId, CreateAccount = createAccount };
            return PostAuthAsync(route, dto, noAuth: true);
        }

        #endregion

        #region Link

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkGuestAsync(bool createAccount = false)
        {
            var route = $"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/guest";
            var dto = new LoginAsGuestDto { CreateAccount = createAccount };

            if (_storage.HasKey(GUESTID_KEY))
            {
                dto.GuestId = _storage.GetString(GUESTID_KEY);
            }

            return PostAuthAsync(route, dto);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkDeviceAsync(string deviceId, bool createAccount = false)
        {
            var route = $"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/device";
            var dto = new LoginByDeviceIdDto { DeviceId = deviceId, CreateAccount = createAccount };
            return PostAuthAsync(route, dto);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkEmailAsync(string email, string password, bool createAccount = false)
        {
            var route = $"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/email";
            var dto = new LoginByEmailDto { Email = email, Password = password, CreateAccount = createAccount };
            return PostAuthAsync(route, dto);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkUsernameAsync(string username, string password, bool createAccount = false)
        {
            var route = $"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/username";
            var dto = new LoginByUsernameDto { Login = username, Password = password, CreateAccount = createAccount };
            return PostAuthAsync(route, dto);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkVkGamesAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/vk-games", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkYandexGamesAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/yandex-games", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkGooglePlayAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/google-play", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkAppleGameCenterAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/apple-game-center", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkGoogleAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/google", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkAppleAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/apple", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkYandexAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/yandex", userId, createAccount);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> LinkOpenIdAsync(string userId, bool createAccount = false)
        {
            return LinkByUserIdAsync($"{AUTH_LINK_ROUTE}/{_configuration.ProjectId}/openid", userId, createAccount);
        }

        private AsyncOperation<RestApiResult<GetAuthDataDto>> LinkByUserIdAsync(string route, string userId, bool createAccount)
        {
            var dto = new LoginByUserIdDto { UserId = userId, CreateAccount = createAccount };
            return PostAuthAsync(route, dto);
        }

        public AsyncOperation<RestApiResult<GetAuthDataDto>> ResolveLinkConflictAsync(int providerType, string targetAccountId)
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/link-conflict/resolve";
            var dto = new LinkAuthProviderDto { ProviderType = providerType, TargetAccountId = targetAccountId };
            return PostAuthAsync(route, dto);
        }

        #endregion

        #region Session

        public AsyncOperation<RestApiResult> RefreshSessionAsync()
        {
            if (string.IsNullOrEmpty(_refreshToken))
            {
                _logger.Log("RefreshSessionAsync called without refresh token.");
                return AsyncOperation<RestApiResult>.CreateCompleted(RestApiResult.ValidationFail("Refresh token is empty."));
            }

            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/refresh";
            var dto = new RefreshSessionDto { RefreshToken = _refreshToken };

            var refreshOp = _restApi.PostAsync<SessionRefreshResultDto>(route, dto, new RestRequestConfig { NoAuth = true, DisableRetry = true });
            var resultOp = new AsyncOperation<RestApiResult>();

            refreshOp.UseCompleted(completed =>
            {
                if (!completed.Result.IsSuccess || completed.Result.Data?.Session == null)
                {
                    HandleSessionExpired();
                    resultOp.Complete(completed.Result.IsSuccess
                        ? RestApiResult.Fail(RestApiError.Validation("Refresh response without session."))
                        : completed.Result);
                    return;
                }

                ApplySession(completed.Result.Data.Session);
                SaveSessionToStorage();
                OnSessionRefreshed?.Invoke();
                resultOp.Complete(RestApiResult.Success());
            });

            return resultOp;
        }

        public AsyncOperation<RestApiResult> LogoutAsync()
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/logout";
            var dto = new LogoutSessionDto { SessionId = _sessionId };

            var op = _restApi.PostAsync(route, dto);
            op.UseCompleted(_ =>
            {
                ClearSessionAndStorage();
                OnSessionExpired?.Invoke();
            });
            return op;
        }

        public AsyncOperation<RestApiResult> LogoutAllAsync()
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/logout/all";
            var op = _restApi.PostAsync(route, new { });
            op.UseCompleted(_ =>
            {
                ClearSessionAndStorage();
                OnSessionExpired?.Invoke();
            });
            return op;
        }

        #endregion

        #region Internal handlers

        private AsyncOperation<RestApiResult<GetAuthDataDto>> PostAuthAsync(string route, object dto, bool noAuth = false)
        {
            var config = noAuth ? new RestRequestConfig { NoAuth = true, DisableRetry = true } : null;
            var op = _restApi.PostAsync<GetAuthDataDto>(route, dto, config);
            op.UseCompleted(HandleAuthCompleted); ;
            return op;
        }

        private AsyncOperation<RestApiResult<GetAuthDataDto>> GetAuthAsync(string route, bool noAuth = false)
        {
            var config = noAuth ? new RestRequestConfig { NoAuth = true, DisableRetry = true } : null;
            var operation = _restApi.GetAsync<GetAuthDataDto>(route, config);
            
            operation.UseCompleted(HandleAuthCompleted);
            return operation;
        }

        private void HandleAuthCompleted(IAsyncOperation<RestApiResult<GetAuthDataDto>> operation)
        {
            _logger.Log("handle auth");
            var result = operation.Result;
            
            if (!result.IsSuccess)
            {
                _logger.Error(result.Error?.Message ?? "Auth request failed.");
                return;
            }

            var data = result.Data;
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

        private void ClearSessionAndStorage()
        {
            ClearSession();
            ClearSessionStorage();
        }

        private RestRequestConfig AuthTokenInterceptor(RestRequestConfig config)
        {
            if (config.NoAuth == true)
            {
                return config;
            }
            
            if (!string.IsNullOrEmpty(_authToken))
            {
                config.Headers ??= new Dictionary<string, string>();
                config.Headers["Authorization"] = "Bearer " + _authToken;
            }

            return config;
        }

        private void HandleSessionExpired()
        {
            ClearSessionAndStorage();
            OnSessionExpired?.Invoke();
        }

        bool ISessionRefresher.CanRefresh => string.IsNullOrEmpty(_refreshToken) == false;

        AsyncOperation<RestApiResult> ISessionRefresher.RefreshSessionAsync()
        {
            return RefreshSessionAsync();
        }

        #endregion

        public void CloudSdkInitialize() { }
        public void CloudSdkDispose() { }
    }
}
