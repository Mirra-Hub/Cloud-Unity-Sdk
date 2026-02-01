using System.Collections.Generic;
using System.Threading.Tasks;
using MirraCloud.Core;
using UnityEngine;

namespace MirraCloud.Example
{
    public class EconomyPanelUI : MonoBehaviour
    {
        [SerializeField] private ItemResourceUI _itemResourceUIPrefab;
        [SerializeField] private RectTransform _itemResourceContainer;
        
        private readonly List<ItemResourceUI> _itemResourcesUi = new List<ItemResourceUI>();

        public async void LoadEconomy()
        {
            await LoadEconomyAsync();
        }
        
        private async Task LoadEconomyAsync()
        {
            if (MirraCloudSDK.IsInitialized == false)
            {
                return;
            }
            
            foreach (var childItem in _itemResourcesUi)
            {
                Destroy(childItem.gameObject);
            }
            
            _itemResourcesUi.Clear();
            
            var operation = MirraCloudSDK.Economy.GetConfigsAsync();

            await operation.Task();

            if (operation.Result.IsSuccess)
            {
                foreach (var item in MirraCloudSDK.Economy.Items)
                {
                    var itemUI = Instantiate(_itemResourceUIPrefab, _itemResourceContainer);
                    itemUI.Initialize(item.Key, item.Value);
                    
                    _itemResourcesUi.Add(itemUI);
                }
            }
        }
    }
}
