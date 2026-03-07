using System;
using MirraCloud.Core.CloudSave.Requests;
using MirraCloud.Core.CloudSave.Responses;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using UnityEngine.Networking;

namespace MirraCloud.Core.CloudSave
{
    public class CloudSaveService
    {
        private const string ControllerApi = "/cloud-save/v1";

        private readonly IJsonService _jsonService;
        private readonly Logger.ILogger _logger;
        private readonly RestApiClient _restApi;
        private readonly Configuration _configuration;

        public PlayerData PlayerData { get; private set; }

        public CloudSaveService(Configuration configuration, Logger.ILogger logger, IJsonService jsonService, RestApiClient restApi)
        {
            _configuration = configuration;
            _restApi = restApi;
            _logger = logger;
            _jsonService = jsonService;
        }

        #region Player Data (Own)

        public AsyncOperation<RestApiResult<DataItemResponse[]>> GetPlayerDataAsync(string[] keys = null)
        {
            string route = BuildDataRoute("player/data", keys);
            var request = _restApi.GetAsync<DataItemResponse[]>(route);

            request.UseCompleted(completed =>
            {
                if (completed.Result.IsSuccess && completed.Result.Data != null)
                {
                    PlayerData = new PlayerData(completed.Result.Data);
                }
            });

            return request;
        }

        public AsyncOperation<RestApiResult> UpsertPlayerDataAsync(CloudSaveDataRequest data)
        {
            string route = BuildDataRoute("player/data");
            return _restApi.PostAsync(route, data);
        }

        public AsyncOperation<RestApiResult> DeletePlayerDataAsync(params string[] keys)
        {
            string route = BuildDataRoute("player/data");
            var config = new RestRequestConfig { Body = new DeleteKeysRequest(keys) };
            return _restApi.DeleteAsync(route, config);
        }

        #endregion

        #region Player Data (Other)

        public AsyncOperation<RestApiResult<DataItemResponse[]>> GetOtherPlayerDataAsync(string playerProfileId, string[] keys = null)
        {
            string route = BuildDataRoute($"players/{playerProfileId}/data", keys);
            return _restApi.GetAsync<DataItemResponse[]>(route);
        }

        public AsyncOperation<RestApiResult> UpsertOtherPlayerDataAsync(string playerProfileId, CloudSaveDataRequest data)
        {
            string route = BuildDataRoute($"players/{playerProfileId}/data");
            return _restApi.PostAsync(route, data);
        }

        public AsyncOperation<RestApiResult> DeleteOtherPlayerDataAsync(string playerProfileId, params string[] keys)
        {
            string route = BuildDataRoute($"players/{playerProfileId}/data");
            var config = new RestRequestConfig { Body = new DeleteKeysRequest(keys) };
            return _restApi.DeleteAsync(route, config);
        }

        #endregion

        #region Global Data

        public AsyncOperation<RestApiResult<DataItemResponse[]>> GetGlobalDataAsync(string[] keys = null)
        {
            string route = BuildDataRoute("global/data", keys);
            return _restApi.GetAsync<DataItemResponse[]>(route);
        }

        public AsyncOperation<RestApiResult> UpsertGlobalDataAsync(CloudSaveDataRequest data)
        {
            string route = BuildDataRoute("global/data");
            return _restApi.PostAsync(route, data);
        }

        public AsyncOperation<RestApiResult> DeleteGlobalDataAsync(params string[] keys)
        {
            string route = BuildDataRoute("global/data");
            var config = new RestRequestConfig { Body = new DeleteKeysRequest(keys) };
            return _restApi.DeleteAsync(route, config);
        }

        #endregion

        #region Backward Compatibility

        public AsyncOperation<RestApiResult<DataItemResponse[]>> LoadAsync()
        {
            return GetPlayerDataAsync();
        }

        public AsyncOperation<RestApiResult> SaveAsync(CloudSaveDataRequest data)
        {
            return UpsertPlayerDataAsync(data);
        }

        #endregion

        #region Route Building

        private string BuildDataRoute(string path, string[] keys = null)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/{path}";

            if (keys == null || keys.Length == 0)
                return route;

            string queryString = string.Join("&", Array.ConvertAll(keys, k => "keys=" + UnityWebRequest.EscapeURL(k)));
            return route + "?" + queryString;
        }

        #endregion
    }
}
