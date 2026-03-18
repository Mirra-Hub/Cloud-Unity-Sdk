using System.Collections.Generic;
using System.Threading.Tasks;
using MirraCloud.Core;
using MirraCloud.Core.Economy.Dto;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby;

namespace Plugins.MirraCloud.Example.Scripts.Core
{
    public class PlayerItem
    {
        public string ItemKey;
        public int Amount;
    }

    public class PlayerEconomy : ILoginInitializable
    {
        private readonly List<PlayerItem> _playerItems = new List<PlayerItem>();
        public IReadOnlyList<PlayerItem> PlayerItems => _playerItems;

        public async Task<bool> Initialize()
        {
            AsyncOperation<RestApiResult<PlayerInventoryDto>> operation = MirraCloudSDK.Economy.LoadInventoryAsync();
            await operation.Task();

            var isSuccess = operation.Result.IsSuccess;

            if (isSuccess && operation.Result.Data?.Items != null)
            {
                foreach (var slot in operation.Result.Data.Items)
                {
                    _playerItems.Add(new PlayerItem()
                    {
                        ItemKey = slot.ItemId,
                        Amount = slot.Quantity,
                    });
                }
            }

            return isSuccess;
        }
    }
}
