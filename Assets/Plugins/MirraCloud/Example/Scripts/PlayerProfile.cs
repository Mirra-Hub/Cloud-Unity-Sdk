using System;
using MirraCloud;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.General.AsyncOperations;

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

        public AsyncOperation<RestApiResult> ChangeNameAsync(string newPlayerName)
        {
            var operation = MirraCloudSDK.PlayerAccount.UpdateNicknameAsync(newPlayerName);

            operation.OnCompleted += completed =>
            {
                if (completed.Result.IsSuccess)
                {
                    Name = newPlayerName;
                    OnPlayerInfoChanged?.Invoke();
                }
            };

            return operation;
        }
    }
}
