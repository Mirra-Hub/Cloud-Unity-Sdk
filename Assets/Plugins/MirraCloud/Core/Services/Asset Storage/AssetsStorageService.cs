using System.Collections.Generic;
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

        public IRestApiOperation LoadConfigAsync()
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/config";
            
            var response = _restApi.Get(route);
            
            _assets.Clear();
            _folders.Clear();
            
            response.UseCompletedCallback(result =>
            {
                var structureDto = result.GetData<AssetStorageStructureDto>();

                Debug.Log(response.DownloadHandler.text);
                
                AddStorageItems(structureDto);
            });
            
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
        
        public IRestApiOperation<TextFile> LoadTextFromId(string id, ExtractTextFileType textFileType = ExtractTextFileType.Text)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/assets/{id}";

            var response = _restApi.Get<TextFile>(route);
            
            response.UseExtractData((operation => ExtractTextFile(operation, textFileType)));
            
            return response;
        }

        public IRestApiOperation<Texture2D> LoadTextureFromId(string id, bool readable = false)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/assets/{id}";
            
            RequestOptions options = new RequestOptions()
            {
                DownloadHandler = new DownloadHandlerTexture(readable),
            };
            
            var response = _restApi.Get<Texture2D>(route, options);
            
            response.UseExtractData(ExtractTexture);
            
            return response;
        }


        public IRestApiOperation<AudioClip> LoadAudioFromId(string id, AudioType audioType)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/assets/{id}";
            
            RequestOptions options = new RequestOptions()
            {
                DownloadHandler = new DownloadHandlerAudioClip(_restApi.GetUrl(route), audioType),
            };
            
            var response = _restApi.Get<AudioClip>(route, options);
            
            response.UseExtractData(ExtractAudio);
            
            return response;
        }
        
        public IRestApiOperation<AssetBundle> LoadAssetBundleFromId(string id)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/assets/{id}";
            
            RequestOptions options = new RequestOptions()
            {
                DownloadHandler = new DownloadHandlerAssetBundle(_restApi.GetUrl(route), 0),
            };
            
            var response = _restApi.Get<AssetBundle>(route, options);
            
            response.UseExtractData(ExtractAssetBundle);
            
            return response;
        }

        private AssetBundle ExtractAssetBundle(RestApiOperation<AssetBundle> result)
        {
            if (result.IsSuccess)
            {
                return DownloadHandlerAssetBundle.GetContent(result.WebRequest);
            }

            return null;
        }

        private Texture2D ExtractTexture(RestApiOperation<Texture2D> result)
        {
            if (result.IsSuccess)
            {
                return DownloadHandlerTexture.GetContent(result.WebRequest);
            }

            return null;
        }
        
        
        private TextFile ExtractTextFile(RestApiOperation<TextFile> result, ExtractTextFileType extractType)
        {
            if (result.IsSuccess)
            {
                var textFile = new TextFile();

                if (extractType == ExtractTextFileType.All || extractType == ExtractTextFileType.Text)
                {
                    textFile.Text = result.DownloadHandler.text;
                }

                if (extractType == ExtractTextFileType.All || extractType == ExtractTextFileType.Data)
                {
                    textFile.Data = result.DownloadHandler.data;
                }

                return textFile;
               
            }

            return null;
        }

        private AudioClip ExtractAudio(RestApiOperation<AudioClip> result)
        {
            if (result.IsSuccess)
            {
                return DownloadHandlerAudioClip.GetContent(result.WebRequest);
            }

            return null;
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
