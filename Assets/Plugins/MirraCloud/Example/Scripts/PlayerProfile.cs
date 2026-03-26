using System;
using System.Threading.Tasks;
using MirraCloud.Core;
using Plugins.MirraCloud.Core.General.AsyncOperations;
using Plugins.MirraCloud.Example.Scripts.Infrastructure.Lobby;
using UnityEngine;

namespace Plugins.MirraCloud.Example.Scripts
{
    public class PlayerProfile : ILoginInitializable
    {
        private readonly IMirraCloudSdk _sdk;

        public string Name { get; private set; }
        public string PlayerId { get; private set; }
        public event Action OnPlayerInfoChanged;

        public PlayerProfile(IMirraCloudSdk sdk)
        {
            _sdk = sdk;
        }

        public async Task<bool> Initialize()
        {
            Debug.Log($"Initializing player profile {_sdk.PlayerAccount.PlayerAccountInfo}");

            if (_sdk.PlayerAccount.PlayerAccountInfo != null)
            {
                Name = _sdk.PlayerAccount.PlayerAccountInfo.Nickname;
                PlayerId = _sdk.PlayerAccount.PlayerAccountInfo.Id;
                return true;
            }

            return false;
        }

        public AsyncOperation<RestApiResult> ChangeNameAsync(string newPlayerName)
        {
            var operation = _sdk.PlayerAccount.UpdateNicknameAsync(newPlayerName);

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
