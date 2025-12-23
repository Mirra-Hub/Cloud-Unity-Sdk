using System;
using System.Collections.Generic;
using MirraCloud;
using MirraCloud.Core;
using MirraCloud.Core.Auth;
using MirraCloud.Core.Logger;
using Plugins.MirraCloud.Core.Services.PlayerAccount.Dto;

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
        public IReadOnlyList<ProfileInfo> Profiles => _profiles;

        private List<ProfileInfo> _profiles = new();

        public event Action<IReadOnlyList<ProfileInfo>> OnProfilesChanged;
        public event Action<ProfileInfo> OnProfileUpdated;
        public event Action<string> OnProfileSelected;

        public PlayerAccountService(AuthenticationService authenticationService, RestApiClient restApi, Configuration configuration, ILogger logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
            _restApi = restApi;
            _configuration = configuration;

            _authenticationService.OnLogin += OnAuthLogin;
            _restApi.UseRequestInterceptor(MetaDataHeadersInterceptor);
        }

        public void Dispose()
        {
            _authenticationService.OnLogin -= OnAuthLogin;
        }
        
        private RestRequestConfig MetaDataHeadersInterceptor(RestRequestConfig config)
        {
            //  new Claim(nameof(profile.Username), profile?.Username ?? string.Empty),
            //       new Claim(nameof(profile.IconKey), JsonConvert.SerializeObject(profile?.IconKey ?? new IconKey()))
            //  new Claim(nameof(account.Age), account.Age.ToString()),
            //   new Claim(nameof(account.Country), account.Country.ToString()),
            //  new Claim(nameof(account.LanguageCode), account.LanguageCode.ToString()),
            //     new Claim("Account" + nameof(account.SegmentIds), string.Join(',', account.SegmentIds)),
            //    new Claim("Profile" + nameof(profile.SegmentIds), string.Join(',', profile?.SegmentIds ?? [])),
            
            if (PlayerAccountInfo != null)
            {
                config.Headers ??= new Dictionary<string, string>();
                config.Headers["Username"] = PlayerAccountInfo.Nickname;
                config.Headers["IconKey"] = "";
                config.Headers["Age"] = PlayerAccountInfo.Age.ToString();
                config.Headers["Country"] = PlayerAccountInfo.Country;
                config.Headers["AccountSegmentIds"] =  string.Join(',', PlayerAccountInfo.SegmentIds);
                config.Headers["ProfileSegmentIds"] = string.Join(',', PlayerAccountInfo.SegmentIds);
            }

            return config;
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
            var operation = _restApi.Patch(route, dto);

            operation.UseCompletedCallback(x =>
            {
                if (x.IsSuccess)
                {
                    PlayerAccountInfo.Nickname = nickname;
                }
            });

            return operation;
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

        public RestApiOperation<ProfileInfo[]> GetProfilesAsync()
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles";
            var op = _restApi.Get<ProfileInfo[]>(route);

            op.UseExtractDataCallback(response =>
            {
                var data = response.GetData<ProfileInfo[]>() ?? Array.Empty<ProfileInfo>();
                _profiles = new List<ProfileInfo>(data);
                OnProfilesChanged?.Invoke(Profiles);
                return data;
            });

            return op;
        }

        public RestApiOperation<ProfileInfo> GetProfileAsync(string profileId)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}";
            var op = _restApi.Get<ProfileInfo>(route);

            op.UseCompletedCallback(operation =>
            {
                if (operation.IsSuccess)
                {
                    var profile = operation.Value;
                    UpdateLocalProfile(profile);
                }
            });

            return op;
        }

        public RestApiOperation<ProfileInfo> CreateProfileAsync(CreateProfileDto dto, bool autoSelect = false)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles?autoSelect={autoSelect}";
            var op = _restApi.Post<ProfileInfo>(route, dto);

            op.UseCompletedCallback(operation =>
            {
                if (operation.IsSuccess && operation.Value != null)
                {
                    _profiles.Add(operation.Value);
                    OnProfilesChanged?.Invoke(Profiles);
                    OnProfileUpdated?.Invoke(operation.Value);
                }
            });

            return op;
        }

        public RestApiOperation DeleteProfileAsync(string profileId)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}";
            var op = _restApi.Delete(route);

            op.UseCompletedCallback(operation =>
            {
                if (operation.IsSuccess)
                {
                    _profiles.RemoveAll(p => p.Id == profileId);
                    OnProfilesChanged?.Invoke(Profiles);
                }
            });

            return op;
        }

        public RestApiOperation UpdateProfileNicknameAsync(string profileId, string username)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}/nickname";
            var dto = new UpdateProfileNicknameDto { Username = username };
            var op = _restApi.Patch(route, dto);

            op.UseCompletedCallback(operation =>
            {
                if (operation.IsSuccess)
                {
                    RefreshProfile(profileId);
                }
            });

            return op;
        }

        public RestApiOperation UpdateProfileIconAsync(string profileId, string iconKeyJson)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}/icon";
            var dto = new UpdateProfileIconDto { IconKeyJson = iconKeyJson };
            var op = _restApi.Patch(route, dto);

            op.UseCompletedCallback(operation =>
            {
                if (operation.IsSuccess)
                {
                    RefreshProfile(profileId);
                }
            });

            return op;
        }

        public RestApiOperation UpdateProfileSegmentsAsync(string profileId, string[] segmentIds)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}/segments";
            var dto = new UpdateProfileSegmentsDto { SegmentIds = segmentIds };
            var op = _restApi.Patch(route, dto);

            op.UseCompletedCallback(operation =>
            {
                if (operation.IsSuccess)
                {
                    RefreshProfile(profileId);
                }
            });

            return op;
        }

        public RestApiOperation UpdateProfileStatusAsync(string profileId, string status)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}/status";
            var dto = new UpdateProfileStatusDto { Status = status };
            var op = _restApi.Patch(route, dto);

            op.UseCompletedCallback(operation =>
            {
                if (operation.IsSuccess)
                {
                    RefreshProfile(profileId);
                }
            });

            return op;
        }

        public RestApiOperation<ProfileInfo> ReplaceProfileAsync(string profileId, CreateProfileDto dto)
        {
            var route = $"{PROFILES_ROUTE}/{_configuration.ProjectId}/profiles/{profileId}";
            var op = _restApi.Put<ProfileInfo>(route, dto);

            op.UseCompletedCallback(operation =>
            {
                if (operation.IsSuccess && operation.Value != null)
                {
                    UpdateLocalProfile(operation.Value);
                }
            });

            return op;
        }

        private void RefreshProfile(string profileId)
        {
            var _ = GetProfileAsync(profileId);
        }

        private void UpdateLocalProfile(ProfileInfo profile)
        {
            if (profile == null)
            {
                return;
            }

            var index = _profiles.FindIndex(p => p.Id == profile.Id);
            if (index >= 0)
            {
                _profiles[index] = profile;
            }
            else
            {
                _profiles.Add(profile);
            }

            OnProfilesChanged?.Invoke(Profiles);
            OnProfileUpdated?.Invoke(profile);
        }

        public RestApiOperation SelectProfileAsync(string profileId)
        {
            var route = $"{ACCOUNTS_ROUTE}/{_configuration.ProjectId}/accounts/profile";
            var dto = new { ProfileId = profileId };
            var op = _restApi.Patch(route, dto);

            op.UseCompletedCallback(operation =>
            {
                if (operation.IsSuccess)
                {
                    OnProfileSelected?.Invoke(profileId);
                }
            });

            return op;
        }

        #endregion
    }
}
