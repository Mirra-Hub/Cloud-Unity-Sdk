using System.Collections.Generic;
using System.Threading.Tasks;
using MirraCloud.Core;
using MirraCloud.Example.Infrastructure.DI;
using UnityEngine;

namespace MirraCloud.Example
{
    public class EconomyPanelUI : MonoBehaviour
    {
        [SerializeField] private ItemResourceUI _itemResourceUIPrefab;
        [SerializeField] private RectTransform _itemResourceContainer;

        private IMirraCloudSdk _sdk;
        private readonly List<ItemResourceUI> _itemResourcesUi = new List<ItemResourceUI>();

        [InjectDep]
        public void Construct(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        public async void LoadEconomy()
        {
            await LoadEconomyAsync();
        }

        private async Task LoadEconomyAsync()
        {
            if (_sdk.IsInitialized == false)
            {
                return;
            }

            foreach (var childItem in _itemResourcesUi)
            {
                Destroy(childItem.gameObject);
            }

            _itemResourcesUi.Clear();

            var operation = _sdk.Economy.LoadConfigsAsync();

            await operation.Task();

            if (operation.Result.IsSuccess)
            {
                foreach (var item in _sdk.Economy.Items)
                {
                    var itemUI = Instantiate(_itemResourceUIPrefab, _itemResourceContainer);
                    itemUI.Initialize(item.Key, item.Value);

                    _itemResourcesUi.Add(itemUI);
                }
            }
        }
    }
}
