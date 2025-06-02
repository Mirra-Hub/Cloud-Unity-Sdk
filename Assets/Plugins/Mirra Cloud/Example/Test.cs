using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MirraCloud.Core;
using MirraCloud.Core.AssetsStorage;
using MirraCloud.Core.CloudSave;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _remoteConfigText;
        [SerializeField] private TextMeshProUGUI _cloudSaveText;
        [SerializeField] private ItemResourceUI _itemResourceUIPrefab;
        [SerializeField] private RectTransform _itemResourceContainer;
        
        [SerializeField] private RawImage _assetPrefab;
        [SerializeField] private RectTransform _assetsContainer;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private string _audioAssetId = "27ee5553-3871-494a-833e-2fc5d7900af6";

        private List<ItemResourceUI> _itemResourcesUi = new List<ItemResourceUI>();
        
        private async void Start()
        {
            MirraCloudSDK.Initialize();

           // string deviceId = SystemInfo.deviceUniqueIdentifier;
           string deviceId = "asdsffg45235";
            var authOperation = MirraCloudSDK.Authentication.LoginWithDeviceIDAsync(deviceId);

            await authOperation.Task;

            Debug.Log($"auth completed {MirraCloudSDK.Authentication.IsAuth}");

            if (MirraCloudSDK.Authentication.IsAuth)
            {
               await LoadRemoteConfigAsync();

             //   await LoadCloudSaveAsync();

              //  await LoadAssetsAsync();
                await LoadEconomyAsync();
            }
        }


        public async void SaveData(string key, string value)
        {
            UpdateDataContainer container = new UpdateDataContainer();
            container.AddString(key, value);

            await MirraCloudSDK.CloudSave.SaveAsync(container).Task;
        }


        public async void LoadRemoteConfig()
        {
            await LoadRemoteConfigAsync();
        }

    
        public async void LoadCloudSave()
        {
            await LoadCloudSaveAsync();
        }
 
  
        public async void LoadEconomy()
        {
            await LoadEconomyAsync();
        }

        private async Task LoadRemoteConfigAsync()
        {
            await MirraCloudSDK.RemoteConfig.LoadConfigAsync().Task;

            StringBuilder stringBuilder = new StringBuilder();
            
            if (MirraCloudSDK.RemoteConfig.Config != null)
            {
                foreach (var field in MirraCloudSDK.RemoteConfig.Config.Fields)
                {
                    Debug.Log(field);
                    stringBuilder.AppendLine($"{field.Name}: {field.Value}");
                }
            }

            _remoteConfigText.text = stringBuilder.ToString();
        }

        private async Task LoadCloudSaveAsync()
        {
            await MirraCloudSDK.CloudSave.LoadAsync().Task;

            StringBuilder stringBuilder = new StringBuilder();
            
            if (MirraCloudSDK.CloudSave.PlayerData != null)
            {
                foreach (var field in MirraCloudSDK.CloudSave.PlayerData.Fields)
                {
                    Debug.Log(field);
                    stringBuilder.AppendLine($"{field.Key}: {field.CurrentValue}");
                }
            }

            _cloudSaveText.text = stringBuilder.ToString();
        }


        private async Task LoadAssetsAsync()
        {
            await MirraCloudSDK.AssetsStorage.LoadConfigAsync().Task;
            
            foreach (var asset in MirraCloudSDK.AssetsStorage.Assets)
            {
                if (asset.Type != AssetType.Image)
                {
                     continue;
                }
                
                Instantiate(_assetPrefab, _assetsContainer);
                
         
            }
            
            int counter = 0;
            
            for (int index = 0; index < MirraCloudSDK.AssetsStorage.Assets.Count; index++)
            {
                var asset = MirraCloudSDK.AssetsStorage.Assets[index];

                if (asset.Type != AssetType.Image)
                {
                    continue;
                }
                
                var operation = MirraCloudSDK.AssetsStorage.LoadTextureFromId(asset.ItemId);

                await operation.Task;
                
                
                var child = _assetsContainer.GetChild(counter);


                counter += 1;
                
            
                
                if (operation.IsSuccess)
                {
                    if (child.TryGetComponent(out RawImage image))
                    {
                        var texture = operation.Value;
                        
                        image.texture = texture;
                        image.SetNativeSize();
                    
                        var rt = image.rectTransform;
                        float parentWidth  = ((RectTransform)rt.parent).rect.width;
                        float parentHeight = ((RectTransform)rt.parent).rect.height;
                        float texRatio     = (float)operation.Value.width / operation.Value.height;
                        float parentRatio  = parentWidth / parentHeight;

                        if (texRatio > parentRatio)
                        {
                            rt.sizeDelta = new Vector2(parentWidth, parentWidth / texRatio);
                        }
                        else
                        {
                            rt.sizeDelta = new Vector2(parentHeight * texRatio, parentHeight);
                        }
                    }
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private async void LoadAssetAsync()
        {
            await LoadAssetsAsync();
        }
        
        private async void LoadAudio()
        {
            await LoadAudioAsync();
        }

        private async Task LoadAudioAsync()
        {
            var operation = MirraCloudSDK.AssetsStorage.LoadAudioFromId(_audioAssetId, AudioType.OGGVORBIS);

            await operation.Task;

            if (operation.IsSuccess)
            {
                _audioSource.clip = operation.Value;
                _audioSource.Play();
            }
        }
        
        private async Task LoadEconomyAsync()
        {
            foreach (var childItem in _itemResourcesUi)
            {
                Destroy(childItem.gameObject);
            }
            
            _itemResourcesUi.Clear();
            
            var operation = MirraCloudSDK.Economy.LoadConfigAsync();

            await operation.Task;

            if (operation.IsSuccess)
            {
                foreach (var item in MirraCloudSDK.Economy.Items)
                {
                    var itemUI = Instantiate(_itemResourceUIPrefab, _itemResourceContainer);
                    itemUI.Initialize(item);
                    
                    _itemResourcesUi.Add(itemUI);
                }
            }
        }
    }
}
