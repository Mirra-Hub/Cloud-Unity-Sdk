using System;
using System.Threading.Tasks;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby;

namespace Plugins.MirraCloud.Example.Scripts
{
    public class PlayerProfile : ILoginInitializable
    {
        public string Name { get; private set; }
        public string PlayerId { get; private set; }
        public event Action OnPlayerInfoChanged;

        public async Task<bool> Initialize()
        {
            if (MirraCloudSDK.PlayerAccount.PlayerAccountInfo != null)
            {
                Name = MirraCloudSDK.PlayerAccount.PlayerAccountInfo.Nickname;
                PlayerId = MirraCloudSDK.PlayerAccount.PlayerAccountInfo.Id;
                return true;
            }
            
            return false;
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
