using System.Collections.Generic;
using System.Threading.Tasks;
using MirraCloud.Core;
using MirraCloud.Core.AssetsStorage;
using UnityEngine;
using UnityEngine.UI;

namespace MirraCloud.Example
{
    public class AssetsStoragePanelUI : MonoBehaviour
    {
        [SerializeField] private RawImage _assetPrefab;
        [SerializeField] private RectTransform _assetsContainer;

        private readonly List<GameObject> _itemResourcesUi = new List<GameObject>();
        
        public async void LoadAssets()
        {
            await LoadAssetsAsync();
        }
        
        private async Task LoadAssetsAsync()
        {
            foreach (var childItem in _itemResourcesUi)
            {
                Destroy(childItem.gameObject);
            }
            
            _itemResourcesUi.Clear();
            
            await MirraCloudSDK.AssetsStorage.LoadConfigAsync().Task;
            
            foreach (var asset in MirraCloudSDK.AssetsStorage.Assets)
            {
                if (asset.Type != AssetType.Image)
                {
                    continue;
                }
                
                var assetObj = Instantiate(_assetPrefab, _assetsContainer);
                
                _itemResourcesUi.Add(assetObj.gameObject);
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
                
            
                
                if (operation.Result.IsSuccess && operation.Result.Data != null)
                {
                    if (child.TryGetComponent(out RawImage image))
                    {
                        var texture = operation.Result.Data;
                        
                        image.texture = texture;
                        image.SetNativeSize();
                    
                        var rt = image.rectTransform;
                        float parentWidth  = ((RectTransform)rt.parent).rect.width;
                        float parentHeight = ((RectTransform)rt.parent).rect.height;
                        float texRatio     = (float)texture.width / texture.height;
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
    }
}
