using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MirraCloud.Core;
using MirraCloud.Core.Economy.Dto;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts.Core
{
    [Serializable]
    public class EconomyItem
    {
        public string Key;
        public Sprite Icon;

        public event Action OnIconLoaded;

        public void SetIcon(Sprite icon)
        {
            Icon = icon;
            OnIconLoaded?.Invoke();
        }
    }

    public class EconomyService : ILoginInitializable
    {
        private readonly IMirraCloudSdk _sdk;
        private readonly Dictionary<string, EconomyItem> _items = new Dictionary<string, EconomyItem>();
        public IReadOnlyDictionary<string, EconomyItem> Items => _items;

        public EconomyService(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        public async Task<bool> Initialize()
        {
            AsyncOperation<RestApiResult<EconomyConfigsDto>> operation = _sdk.Economy.LoadConfigsAsync();
            await operation.Task();

            bool isSuccess = operation.Result.IsSuccess;

            foreach (var item in _sdk.Economy.Items.Values)
            {
                var economyItem = new EconomyItem()
                {
                    Key = item.Key,
                };

                // string iconUrl = (string)item.Fields["icon"];
                //
                // var loadIconOperation = _sdk.AssetsStorage.LoadSpriteFromId(iconUrl);
                // loadIconOperation.OnCompleted += asyncOperation =>
                // {
                //     economyItem.SetIcon(loadIconOperation.Result.Data);
                // };
                //
                // _items.Add(item.Key, economyItem);
            }

            return isSuccess;
        }

        public EconomyItem GetItem(string itemKey)
        {
            return _items[itemKey];
        }
    }
}
