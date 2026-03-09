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

        public AsyncOperation<RestApiResult<DataItemResponse[]>> GetPlayerDataAsync(string[] keys = null, int? offset = null, int? limit = null)
        {
            string route = BuildDataRoute("player/data", keys, offset, limit);
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

        public AsyncOperation<RestApiResult<DataItemResponse[]>> GetOtherPlayerDataAsync(string playerProfileId, string[] keys = null, int? offset = null, int? limit = null)
        {
            string route = BuildDataRoute($"players/{playerProfileId}/data", keys, offset, limit);
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

        public AsyncOperation<RestApiResult<DataItemResponse[]>> GetGlobalDataAsync(string[] keys = null, int? offset = null, int? limit = null)
        {
            string route = BuildDataRoute("global/data", keys, offset, limit);
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

        #region Custom Data

        public AsyncOperation<RestApiResult<DataItemResponse[]>> GetCustomDataAsync(string customId, string[] keys = null, int? offset = null, int? limit = null)
        {
            string route = BuildDataRoute($"custom/{customId}/data", keys, offset, limit);
            return _restApi.GetAsync<DataItemResponse[]>(route);
        }

        public AsyncOperation<RestApiResult> UpsertCustomDataAsync(string customId, CloudSaveDataRequest data)
        {
            string route = BuildDataRoute($"custom/{customId}/data");
            return _restApi.PostAsync(route, data);
        }

        public AsyncOperation<RestApiResult> DeleteCustomDataAsync(string customId, params string[] keys)
        {
            string route = BuildDataRoute($"custom/{customId}/data");
            var config = new RestRequestConfig { Body = new DeleteKeysRequest(keys) };
            return _restApi.DeleteAsync(route, config);
        }

        #endregion

        #region Query

        public AsyncOperation<RestApiResult<QueryIndexResponse>> QueryPlayerDataAsync(QueryIndexRequest request)
        {
            string route = BuildDataRoute("player/data/query");
            return _restApi.PostAsync<QueryIndexResponse>(route, request);
        }

        public AsyncOperation<RestApiResult<QueryIndexResponse>> QueryGlobalDataAsync(QueryIndexRequest request)
        {
            string route = BuildDataRoute("global/data/query");
            return _restApi.PostAsync<QueryIndexResponse>(route, request);
        }

        public AsyncOperation<RestApiResult<QueryIndexResponse>> QueryCustomDataAsync(string customId, QueryIndexRequest request)
        {
            string route = BuildDataRoute($"custom/{customId}/data/query");
            return _restApi.PostAsync<QueryIndexResponse>(route, request);
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

        private string BuildDataRoute(string path, string[] keys = null, int? offset = null, int? limit = null)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/{path}";

            var parts = new System.Collections.Generic.List<string>();
            if (keys != null && keys.Length > 0)
            {
                foreach (var k in keys)
                    parts.Add("keys=" + UnityWebRequest.EscapeURL(k));
            }
            if (offset.HasValue) parts.Add("offset=" + offset.Value);
            if (limit.HasValue) parts.Add("limit=" + limit.Value);

            if (parts.Count == 0) return route;
            return route + "?" + string.Join("&", parts);
        }

        #endregion
    }
}
