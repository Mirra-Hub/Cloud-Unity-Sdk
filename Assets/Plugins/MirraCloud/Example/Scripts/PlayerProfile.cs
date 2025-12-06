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
            if (MirraCloudSDK.PlayerAccount.PlayerAccountInfo != null)
            {
                Name = MirraCloudSDK.PlayerAccount.PlayerAccountInfo.Nickname;
                PlayerId = MirraCloudSDK.PlayerAccount.PlayerAccountInfo.Id;
            }
        }

        public IBaseRestApiOperation ChangeName(string newPlayerName)
        {
            var operation = MirraCloudSDK.PlayerAccount.UpdateNicknameAsync(newPlayerName);

            operation.OnCompleted += (response =>
            {
                if (response.IsSuccess)
                {
                    Name = newPlayerName;
                    OnPlayerInfoChanged?.Invoke();
                }
            });

            return operation;
        }
    }
}
