using System.Collections.Generic;
using MirraCloud.Core.CloudSave.Responses;
using MirraCloud.Json;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using UnityEngine.Networking;

namespace MirraCloud.Core.CloudSave
{
    public class CloudSaveFilesService
    {
        private const string ControllerApi = "/cloud-save/v1";

        private readonly Logger.ILogger _logger;
        private readonly RestApiClient _restApi;
        private readonly Configuration _configuration;
        private readonly IJsonService _jsonService;

        public CloudSaveFilesService(Configuration configuration, Logger.ILogger logger, IJsonService jsonService, RestApiClient restApi)
        {
            _configuration = configuration;
            _restApi = restApi;
            _logger = logger;
            _jsonService = jsonService;
        }

        #region Player Files (Own)

        public AsyncOperation<RestApiResult<FileItemResponse>> UploadPlayerFileAsync(
            string key, byte[] fileData, string fileName, string mimeType,
            Dictionary<string, string> meta = null,
            AccessMask? readMask = null, AccessMask? writeMask = null)
        {
            string route = BuildFileRoute($"player/files/{UnityWebRequest.EscapeURL(key)}");
            var formSections = BuildUploadForm(fileData, fileName, mimeType, meta, readMask, writeMask);
            var config = new RestRequestConfig { MultipartFormSections = formSections };
            return _restApi.PutAsync<FileItemResponse>(route, null, config);
        }

        public AsyncOperation<RestApiResult<FileItemResponse>> GetPlayerFileAsync(string key)
        {
            string route = BuildFileRoute($"player/files/{UnityWebRequest.EscapeURL(key)}");
            return _restApi.GetAsync<FileItemResponse>(route);
        }

        public AsyncOperation<RestApiResult<FileUrlResponse>> GetPlayerFileUrlAsync(string key)
        {
            string route = BuildFileRoute($"player/files/{UnityWebRequest.EscapeURL(key)}/url");
            return _restApi.GetAsync<FileUrlResponse>(route);
        }

        public AsyncOperation<RestApiResult> DeletePlayerFileAsync(string key)
        {
            string route = BuildFileRoute($"player/files/{UnityWebRequest.EscapeURL(key)}");
            return _restApi.DeleteAsync(route);
        }

        public AsyncOperation<RestApiResult<FileItemResponse>> UpdatePlayerFileMetaAsync(
            string key, Dictionary<string, string> meta)
        {
            string route = BuildFileRoute($"player/files/{UnityWebRequest.EscapeURL(key)}/meta");
            var body = new UpdateFileMetaRequest { meta = meta };
            return _restApi.PatchAsync<FileItemResponse>(route, body);
        }

        public AsyncOperation<RestApiResult<FileItemResponse>> UpdatePlayerFileContentAsync(
            string key, byte[] fileData, string fileName, string mimeType)
        {
            string route = BuildFileRoute($"player/files/{UnityWebRequest.EscapeURL(key)}/content");
            var formSections = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", fileData, fileName, mimeType)
            };
            var config = new RestRequestConfig { MultipartFormSections = formSections };
            return _restApi.PutAsync<FileItemResponse>(route, null, config);
        }

        #endregion

        #region Player Files (Other)

        public AsyncOperation<RestApiResult<FileItemResponse>> GetOtherPlayerFileAsync(string playerProfileId, string key)
        {
            string route = BuildFileRoute($"players/{playerProfileId}/files/{UnityWebRequest.EscapeURL(key)}");
            return _restApi.GetAsync<FileItemResponse>(route);
        }

        public AsyncOperation<RestApiResult<FileUrlResponse>> GetOtherPlayerFileUrlAsync(string playerProfileId, string key)
        {
            string route = BuildFileRoute($"players/{playerProfileId}/files/{UnityWebRequest.EscapeURL(key)}/url");
            return _restApi.GetAsync<FileUrlResponse>(route);
        }

        public AsyncOperation<RestApiResult<FileItemResponse>> UpdateOtherPlayerFileMetaAsync(
            string playerProfileId, string key, Dictionary<string, string> meta)
        {
            string route = BuildFileRoute($"players/{playerProfileId}/files/{UnityWebRequest.EscapeURL(key)}/meta");
            var body = new UpdateFileMetaRequest { meta = meta };
            return _restApi.PatchAsync<FileItemResponse>(route, body);
        }

        public AsyncOperation<RestApiResult<FileItemResponse>> UpdateOtherPlayerFileContentAsync(
            string playerProfileId, string key, byte[] fileData, string fileName, string mimeType)
        {
            string route = BuildFileRoute($"players/{playerProfileId}/files/{UnityWebRequest.EscapeURL(key)}/content");
            var formSections = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", fileData, fileName, mimeType)
            };
            var config = new RestRequestConfig { MultipartFormSections = formSections };
            return _restApi.PutAsync<FileItemResponse>(route, null, config);
        }

        #endregion

        #region Global Files

        public AsyncOperation<RestApiResult<FileItemResponse>> UploadGlobalFileAsync(
            string key, byte[] fileData, string fileName, string mimeType,
            Dictionary<string, string> meta = null,
            AccessMask? readMask = null, AccessMask? writeMask = null)
        {
            string route = BuildFileRoute($"global/files/{UnityWebRequest.EscapeURL(key)}");
            var formSections = BuildUploadForm(fileData, fileName, mimeType, meta, readMask, writeMask);
            var config = new RestRequestConfig { MultipartFormSections = formSections };
            return _restApi.PutAsync<FileItemResponse>(route, null, config);
        }

        public AsyncOperation<RestApiResult<FileItemResponse>> GetGlobalFileAsync(string key)
        {
            string route = BuildFileRoute($"global/files/{UnityWebRequest.EscapeURL(key)}");
            return _restApi.GetAsync<FileItemResponse>(route);
        }

        public AsyncOperation<RestApiResult<FileUrlResponse>> GetGlobalFileUrlAsync(string key)
        {
            string route = BuildFileRoute($"global/files/{UnityWebRequest.EscapeURL(key)}/url");
            return _restApi.GetAsync<FileUrlResponse>(route);
        }

        public AsyncOperation<RestApiResult> DeleteGlobalFileAsync(string key)
        {
            string route = BuildFileRoute($"global/files/{UnityWebRequest.EscapeURL(key)}");
            return _restApi.DeleteAsync(route);
        }

        public AsyncOperation<RestApiResult<FileItemResponse>> UpdateGlobalFileMetaAsync(
            string key, Dictionary<string, string> meta)
        {
            string route = BuildFileRoute($"global/files/{UnityWebRequest.EscapeURL(key)}/meta");
            var body = new UpdateFileMetaRequest { meta = meta };
            return _restApi.PatchAsync<FileItemResponse>(route, body);
        }

        public AsyncOperation<RestApiResult<FileItemResponse>> UpdateGlobalFileContentAsync(
            string key, byte[] fileData, string fileName, string mimeType)
        {
            string route = BuildFileRoute($"global/files/{UnityWebRequest.EscapeURL(key)}/content");
            var formSections = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", fileData, fileName, mimeType)
            };
            var config = new RestRequestConfig { MultipartFormSections = formSections };
            return _restApi.PutAsync<FileItemResponse>(route, null, config);
        }

        #endregion

        #region Private Helpers

        private string BuildFileRoute(string path)
        {
            return $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/{path}";
        }

        private List<IMultipartFormSection> BuildUploadForm(
            byte[] fileData, string fileName, string mimeType,
            Dictionary<string, string> meta,
            AccessMask? readMask, AccessMask? writeMask)
        {
            var sections = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", fileData, fileName, mimeType)
            };

            if (meta != null)
            {
                string metaJson = _jsonService.ToJson(meta);
                sections.Add(new MultipartFormDataSection("metaJson", metaJson));
            }

            if (readMask.HasValue)
            {
                sections.Add(new MultipartFormDataSection("readMask", ((int)readMask.Value).ToString()));
            }

            if (writeMask.HasValue)
            {
                sections.Add(new MultipartFormDataSection("writeMask", ((int)writeMask.Value).ToString()));
            }

            return sections;
        }

        #endregion
    }

    internal class UpdateFileMetaRequest
    {
        public Dictionary<string, string> meta;
    }
}
