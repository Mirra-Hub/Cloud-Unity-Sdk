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
        private static readonly long[] RedirectHttpStatusCodes = { 301, 302, 303, 307, 308 };

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

        private static string ExtractRedirectLocation(UnityWebRequest request)
        {
            return request.GetResponseHeader("Location") ?? request.GetResponseHeader("location");
        }

        private AsyncOperation<RestApiResult<string>> ResolveAssetDownloadUrlAsync(string id)
        {
            string route = $"{ControllerApi}/projects/{_configuration.ProjectId}/branches/{_configuration.BranchId}/assets/{id}";

            var config = new RestRequestConfig
            {
                RedirectLimit = 0,
                AllowedHttpStatusCodes = RedirectHttpStatusCodes
            };

            return _restApi.GetAsync<string>(route, config, ExtractRedirectLocation);
        }
         
        public AsyncOperation<RestApiResult<TextFile>> LoadTextFromId(string id, ExtractTextFileType textFileType = ExtractTextFileType.Text)
        {
            var op = new AsyncOperation<RestApiResult<TextFile>>();

            var resolveOp = ResolveAssetDownloadUrlAsync(id);
            resolveOp.OnCompleted += _ =>
            {
                if (!resolveOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult<TextFile>.Fail(resolveOp.Result.Error).WithMetaFrom(resolveOp.Result));
                    return;
                }

                if (string.IsNullOrWhiteSpace(resolveOp.Result.Data))
                {
                    op.Complete(RestApiResult<TextFile>.ValidationFail("Asset download url is empty.").WithMetaFrom(resolveOp.Result));
                    return;
                }

                var downloadConfig = new RestRequestConfig { NoAuth = true };
                var downloadOp = _restApi.GetAsync<TextFile>(resolveOp.Result.Data, downloadConfig, request =>
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

                downloadOp.OnCompleted += completed => op.Complete(completed.Result);
            };

            return op;
        }

        public AsyncOperation<RestApiResult<Texture2D>> LoadTextureFromId(string id, bool readable = false)
        {
            var op = new AsyncOperation<RestApiResult<Texture2D>>();

            var resolveOp = ResolveAssetDownloadUrlAsync(id);
            resolveOp.OnCompleted += _ =>
            {
                if (!resolveOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult<Texture2D>.Fail(resolveOp.Result.Error).WithMetaFrom(resolveOp.Result));
                    return;
                }

                if (string.IsNullOrWhiteSpace(resolveOp.Result.Data))
                {
                    op.Complete(RestApiResult<Texture2D>.ValidationFail("Asset download url is empty.").WithMetaFrom(resolveOp.Result));
                    return;
                }

                var downloadConfig = new RestRequestConfig
                {
                    NoAuth = true,
                    DownloadHandler = new DownloadHandlerTexture(readable)
                };

                var downloadOp = _restApi.GetAsync<Texture2D>(resolveOp.Result.Data, downloadConfig,
                    request => DownloadHandlerTexture.GetContent(request));

                downloadOp.OnCompleted += completed => op.Complete(completed.Result);
            };

            return op;
        }
        
        public AsyncOperation<RestApiResult<Sprite>> LoadSpriteFromId(string id, bool readable = false)
        {
            var op = new AsyncOperation<RestApiResult<Sprite>>();

            var resolveOp = ResolveAssetDownloadUrlAsync(id);
            resolveOp.UseCompleted(_ =>
            {
                if (!resolveOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult<Sprite>.Fail(resolveOp.Result.Error).WithMetaFrom(resolveOp.Result));
                    return;
                }

                if (string.IsNullOrWhiteSpace(resolveOp.Result.Data))
                {
                    op.Complete(RestApiResult<Sprite>.ValidationFail("Asset download url is empty.")
                        .WithMetaFrom(resolveOp.Result));
                    return;
                }

                var downloadConfig = new RestRequestConfig
                {
                    NoAuth = true,
                    DownloadHandler = new DownloadHandlerTexture(readable)
                };

                var downloadOp = _restApi.GetAsync(resolveOp.Result.Data, downloadConfig, request =>
                {
                    var texture = DownloadHandlerTexture.GetContent(request);
                    
                    Debug.Log($"texture loaded: {texture.texelSize}");
                    
                    return Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * 0.5f);
                });

                downloadOp.OnCompleted += completed => op.Complete(completed.Result);
            });

            return op;
        }


        public AsyncOperation<RestApiResult<AudioClip>> LoadAudioFromId(string id, AudioType audioType)
        {
            var op = new AsyncOperation<RestApiResult<AudioClip>>();

            var resolveOp = ResolveAssetDownloadUrlAsync(id);
            resolveOp.OnCompleted += _ =>
            {
                if (!resolveOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult<AudioClip>.Fail(resolveOp.Result.Error).WithMetaFrom(resolveOp.Result));
                    return;
                }

                if (string.IsNullOrWhiteSpace(resolveOp.Result.Data))
                {
                    op.Complete(RestApiResult<AudioClip>.ValidationFail("Asset download url is empty.").WithMetaFrom(resolveOp.Result));
                    return;
                }

                var downloadUrl = resolveOp.Result.Data;
                var downloadConfig = new RestRequestConfig
                {
                    NoAuth = true,
                    DownloadHandler = new DownloadHandlerAudioClip(downloadUrl, audioType)
                };

                var downloadOp = _restApi.GetAsync<AudioClip>(downloadUrl, downloadConfig,
                    request => DownloadHandlerAudioClip.GetContent(request));

                downloadOp.OnCompleted += completed => op.Complete(completed.Result);
            };

            return op;
        }
        
        public AsyncOperation<RestApiResult<AssetBundle>> LoadAssetBundleFromId(string id)
        {
            var op = new AsyncOperation<RestApiResult<AssetBundle>>();

            var resolveOp = ResolveAssetDownloadUrlAsync(id);
            resolveOp.OnCompleted += _ =>
            {
                if (!resolveOp.Result.IsSuccess)
                {
                    op.Complete(RestApiResult<AssetBundle>.Fail(resolveOp.Result.Error).WithMetaFrom(resolveOp.Result));
                    return;
                }

                if (string.IsNullOrWhiteSpace(resolveOp.Result.Data))
                {
                    op.Complete(RestApiResult<AssetBundle>.ValidationFail("Asset download url is empty.").WithMetaFrom(resolveOp.Result));
                    return;
                }

                var downloadUrl = resolveOp.Result.Data;
                var downloadConfig = new RestRequestConfig
                {
                    NoAuth = true,
                    DownloadHandler = new DownloadHandlerAssetBundle(downloadUrl, 0)
                };

                var downloadOp = _restApi.GetAsync<AssetBundle>(downloadUrl, downloadConfig,
                    request => DownloadHandlerAssetBundle.GetContent(request));

                downloadOp.OnCompleted += completed => op.Complete(completed.Result);
            };

            return op;
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
