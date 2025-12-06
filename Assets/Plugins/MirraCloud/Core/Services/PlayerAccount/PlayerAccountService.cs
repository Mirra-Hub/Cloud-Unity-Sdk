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
        private readonly AuthenticationService _authenticationService;

        private const string ACCOUNTS_ROUTE = "/player-accounts/v1/projects";
        private const string PROFILES_ROUTE = "/player-accounts/v1";

        public PlayerAccountInfo PlayerAccountInfo { get; private set; }

        public PlayerAccountService(AuthenticationService authenticationService, RestApiClient restApi, Configuration configuration, ILogger logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
            _restApi = restApi;
            _configuration = configuration;

            _authenticationService.OnLogin += OnAuthLogin;
        }

        public void Dispose()
        {
            _authenticationService.OnLogin -= OnAuthLogin;
        }

        private void OnAuthLogin(GetAuthDataDto authData)
        {
            var account = authData.CurrentAccount ?? authData.PlayerInfo;
            if (account == null)
            {
                _logger.Log("AuthData does not contain account info.");
                return;
            }

            PlayerAccountInfo = new PlayerAccountInfo(account);
        }

        #region Account

        public RestApiOperation<PlayerAccountInfo> GetAccountAsync()
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/accounts";
            var op = _restApi.Get<PlayerAccountInfo>(route);

            op.UseExtractDataCallback(response =>
            {
                var dto = response.GetData<AccountDto>();
                if (dto == null)
                {
                    return PlayerAccountInfo;
                }

                PlayerAccountInfo = new PlayerAccountInfo(dto);
                return PlayerAccountInfo;
            });

            return op;
        }

        public RestApiOperation UpdateNicknameAsync(string nickname)
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/accounts/nickname";
            var dto = new { Nickname = nickname };
            return _restApi.Patch(route, dto);
        }

        public RestApiOperation UpdateAgeAsync(int age)
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/accounts/age";
            var dto = new { Age = age };
            return _restApi.Patch(route, dto);
        }

        public RestApiOperation UpdateCountryAsync(string country)
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/accounts/country";
            var dto = new { Country = country };
            return _restApi.Patch(route, dto);
        }

        public RestApiOperation UpdateLanguageAsync(string languageCode)
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/accounts/language";
            var dto = new { LanguageCode = languageCode };
            return _restApi.Patch(route, dto);
        }

        public RestApiOperation SelectProfileAsync(string profileId)
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/accounts/profile";
            var dto = new { ProfileId = profileId };
            return _restApi.Patch(route, dto);
        }

        public RestApiOperation UpdateIconAsync(string iconKeyJson)
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/accounts/icon";
            var dto = new { IconKey = iconKeyJson };
            return _restApi.Patch(route, dto);
        }

        public RestApiOperation UpdateSegmentsAsync(string[] segmentIds)
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/accounts/segments";
            var dto = new { SegmentIds = segmentIds };
            return _restApi.Patch(route, dto);
        }

        #endregion

        #region Profiles

        public RestApiOperation GetProfilesAsync()
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles";
            return _restApi.Get(route);
        }

        public RestApiOperation GetProfileAsync(string profileId)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}";
            return _restApi.Get(route);
        }

        public RestApiOperation CreateProfileAsync(object createProfileDto, bool autoSelect = false)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles?autoSelect={autoSelect}";
            return _restApi.Post(route, createProfileDto);
        }

        public RestApiOperation DeleteProfileAsync(string profileId)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}";
            return _restApi.Delete(route);
        }

        public RestApiOperation UpdateProfileNicknameAsync(string profileId, string username)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}/nickname";
            var dto = new { Username = username };
            return _restApi.Patch(route, dto);
        }

        public RestApiOperation UpdateProfileIconAsync(string profileId, string iconKeyJson)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}/icon";
            var dto = new { IconKey = iconKeyJson };
            return _restApi.Patch(route, dto);
        }

        public RestApiOperation UpdateProfileSegmentsAsync(string profileId, string[] segmentIds)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}/segments";
            var dto = new { SegmentIds = segmentIds };
            return _restApi.Patch(route, dto);
        }

        public RestApiOperation ReplaceProfileAsync(string profileId, object createProfileDto)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}";
            return _restApi.Put(route, createProfileDto);
        }

        #endregion
    }
}

