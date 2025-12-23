using System.Collections.Generic;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using UnityEngine;
using UnityEngine.Networking;
using ILogger = MirraCloud.Core.Logger.ILogger;

namespace MirraCloud.Core.AssetsStorage
{
    public class AssetsStorageService 
    {
        private const string ControllerApi = "/assets/v1";

        private readonly Configuration _configuration;
        private readonly RestApiClient _restApi;
        private readonly ILogger _logger;

        private readonly List<Asset> _assets = new List<Asset>();
        private readonly List<Folder> _folders = new List<Folder>();

        public IReadOnlyList<Asset> Assets => _assets;
        public IReadOnlyList<Folder> Folders => _folders;
        
        public AssetsStorageService(Configuration configuration, RestApiClient restApi, ILogger logger)
        {
            _configuration = configuration;
            _restApi = restApi;
            _logger = logger;
        }

        public AsyncOperation<RestApiResult<AssetStorageStructureDto>> LoadConfigAsync()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/config";
            
            var response = _restApi.GetAsync<AssetStorageStructureDto>(route);
            
            _assets.Clear();
            _folders.Clear();
            
            response.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess && completed.Result.Data != null)
                {
                    AddStorageItems(completed.Result.Data);
                }
            };
            
            return response;
        }

        public List<AssetDto> GetAssetsFromType(AssetType assetType)
        {
            List<AssetDto> assets = new List<AssetDto>();

            foreach (var asset in assets)
            {
                if (asset.type == assetType)
                {
                    assets.Add(asset);
                }
            }
            
            return assets;
        }
        
        public AsyncOperation<RestApiResult<TextFile>> LoadTextFromId(string id, ExtractTextFileType textFileType = ExtractTextFileType.Text)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/assets/{id}";

            return _restApi.GetAsync<TextFile>(route, null, request =>
            {
                var textFile = new TextFile();

                if (textFileType == ExtractTextFileType.All || textFileType == ExtractTextFileType.Text)
                {
                    textFile.Text = request.downloadHandler.text;
                }

                if (textFileType == ExtractTextFileType.All || textFileType == ExtractTextFileType.Data)
                {
                    textFile.Data = request.downloadHandler.data;
                }

                return textFile;
            });
        }

        public AsyncOperation<RestApiResult<Texture2D>> LoadTextureFromId(string id, bool readable = false)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/assets/{id}";
            
            var config = new RestRequestConfig
            {
                DownloadHandler = new DownloadHandlerTexture(readable)
            };
            
            return _restApi.GetAsync<Texture2D>(route, config, request => DownloadHandlerTexture.GetContent(request));
        }


        public AsyncOperation<RestApiResult<AudioClip>> LoadAudioFromId(string id, AudioType audioType)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/assets/{id}";
            
            var config = new RestRequestConfig
            {
                DownloadHandler = new DownloadHandlerAudioClip(_restApi.GetUrl(route), audioType),
            };
            
            return _restApi.GetAsync<AudioClip>(route, config, request => DownloadHandlerAudioClip.GetContent(request));
        }
        
        public AsyncOperation<RestApiResult<AssetBundle>> LoadAssetBundleFromId(string id)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/assets/{id}";
            
            var config = new RestRequestConfig
            {
                DownloadHandler = new DownloadHandlerAssetBundle(_restApi.GetUrl(route), 0),
            };
            
            return _restApi.GetAsync<AssetBundle>(route, config, request => DownloadHandlerAssetBundle.GetContent(request));
        }

        private void AddStorageItems(AssetStorageStructureDto structureDto)
        {
            if (structureDto.assets != null)
            {
                foreach (var assetDto in structureDto.assets)
                {
                    _assets.Add(new Asset(assetDto));
                }
            }
            
            if (structureDto.folders != null)
            {
                foreach (var folderDto in structureDto.folders)
                {
                    _folders.Add(new Folder(folderDto));
                }
            }
        }
    }
}
