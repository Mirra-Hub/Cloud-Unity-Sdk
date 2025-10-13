using System;
using MirraCloud;
using MirraCloud.Core;

namespace Plugins.MirraCloud.Example.Scripts
{
    public class PlayerProfile 
    {
        public string Name { get; private set; }
        public string PlayerId { get; private set; }
        public event Action OnPlayerInfoChanged;

        public void Initialize()
        {
            Name = MirraCloudSDK.PlayerAccount.PlayerAccountInfo.Name;
            PlayerId = MirraCloudSDK.PlayerAccount.PlayerAccountInfo.PlayerId;
        }

        public IBaseRestApiOperation ChangeName(string newPlayerName)
        {
            var operation = MirraCloudSDK.PlayerAccount.UpdatePlayerInfo(new UpdatePlayerInfoOptions()
            {
                Name = newPlayerName,
            });

            operation.OnCompleted += (response =>
            {
                if (response.IsSuccess)
                {
                    Name = response.Value.Name;
                    
                    OnPlayerInfoChanged?.Invoke();
                }
            });

            return operation;
        }
    }
}
